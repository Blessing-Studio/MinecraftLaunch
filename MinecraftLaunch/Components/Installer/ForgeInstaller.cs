using Flurl.Http;
using System.Text.Json;
using System.Diagnostics;
using System.IO.Compression;
using MinecraftLaunch.Extensions;
using MinecraftLaunch.Components.Resolver;
using MinecraftLaunch.Classes.Models.Game;
using MinecraftLaunch.Classes.Models.Install;
using MinecraftLaunch.Classes.Models.Download;

namespace MinecraftLaunch.Components.Installer;

public sealed class ForgeInstaller(GameEntry inheritedFrom, ForgeInstallEntry installEntry, string javaPath, string customId = default, MirrorDownloadSource mirror = default) : InstallerBase {
    private readonly string _customId = customId;
    private readonly string _javaPath = javaPath;
    private readonly GameEntry _inheritedFrom = inheritedFrom;
    private readonly ForgeInstallEntry _installEntry = installEntry;
    private readonly MirrorDownloadSource _mirrorDownloadSource = mirror;

    public override async ValueTask<bool> InstallAsync() {
        List<HighVersionForgeProcessorEntry> highVersionForgeProcessors = default;

        /*
         * Download Forge installation package
         */
        var suffix = $"/net/minecraftforge/forge/{_installEntry.McVersion}-{_installEntry.ForgeVersion}/forge-{_installEntry
            .McVersion}-{_installEntry.ForgeVersion}-installer.jar";

        var host = MirrorDownloadManager.IsUseMirrorDownloadSource 
            ? _mirrorDownloadSource.Host
            : "https://files.minecraftforge.net/maven";
        var packageUrl = $"{host}{suffix}";

        string packagePath = Path.Combine(Path.GetTempPath(), 
            Path.GetFileName(packageUrl));

        var request = packageUrl.ToDownloadRequest(packagePath.ToFileInfo());
        await request.DownloadAsync(x => {
            ReportProgress(x.ToPercentage(0.0d, 0.15d),
                "Downloading Forge installation package", 
                TaskStatus.Running);
        });

        /*
         * Parse package
         */
        ReportProgress(0.15d, "Start parse package", TaskStatus.Created);
        var packageArchive = ZipFile.OpenRead(request.FileInfo.FullName);
        var installProfile = packageArchive.GetEntry("install_profile.json")
            .ReadAsString()
            .AsNode();
        var isLegacyForgeVersion = installProfile.Select("install") != null;
        var forgeVersion = isLegacyForgeVersion
            ? installProfile.Select("install").GetString("version").Replace("forge ", string.Empty)
            : installProfile.GetString("version").Replace("-forge-", "-");

        var versionInfoJson = isLegacyForgeVersion
            ? installProfile.Select("versionInfo")
            : packageArchive.GetEntry("version.json").ReadAsString().AsNode();

        var libraries = LibrariesResolver.GetLibrariesFromJsonArray(versionInfoJson
                .GetEnumerable("libraries"),
            _inheritedFrom.GameFolderPath).ToList();

        if (MirrorDownloadManager.IsUseMirrorDownloadSource) {
            foreach (var lib in libraries) {
                lib.Url = $"https://bmclapi2.bangbang93.com/maven/{lib.RelativePath.Replace("\\", "/")}";
            }
        }

        if (!isLegacyForgeVersion) {
            libraries.AddRange(LibrariesResolver.GetLibrariesFromJsonArray(installProfile
                    .GetEnumerable("libraries"), 
                _inheritedFrom.GameFolderPath));

            var highVersionForgeDataDictionary = installProfile
                .Select("data")
                .Deserialize<Dictionary<string, Dictionary<string, string>>>();

            if (highVersionForgeDataDictionary.Any()) {
                highVersionForgeDataDictionary["BINPATCH"]["client"] =
                    $"[net.minecraftforge:forge:{forgeVersion}:clientdata@lzma]";

                highVersionForgeDataDictionary["BINPATCH"]["server"] =
                    $"[net.minecraftforge:forge:{forgeVersion}:serverdata@lzma]";
            }

            var replaceValues = new Dictionary<string, string> {
                { "{SIDE}", "client" },
                { "{MINECRAFT_JAR}", _inheritedFrom.JarPath.ToPath() },
                { "{MINECRAFT_VERSION}", installProfile.GetString("minecraft") },
                { "{ROOT}", _inheritedFrom.GameFolderPath.ToPath() },
                { "{INSTALLER}", packagePath.ToPath() },
                { "{LIBRARY_DIR}", Path.Combine(_inheritedFrom.GameFolderPath, "libraries").ToPath() }
            };

            var replaceProcessorArgs = highVersionForgeDataDictionary.ToDictionary(
                kvp => $"{{{kvp.Key}}}", kvp => {
                    var value = kvp.Value["client"];
                    if (!value.StartsWith('[')) {
                        return value;
                    }
                    return Path.Combine(_inheritedFrom.GameFolderPath,
                            "libraries",
                            LibrariesResolver.FormatLibraryNameToRelativePath(value.TrimStart('[').TrimEnd(']')))
                        .ToPath();
                });


            highVersionForgeProcessors = installProfile["processors"]
                .Deserialize<IEnumerable<HighVersionForgeProcessorEntry>>()
                .Where(x => !(x.Sides.Count == 1 && x.Sides.Contains("server")))
                .ToList();

            foreach (var processor in highVersionForgeProcessors) {
                processor.Args = processor.Args.Select(x => {
                    if (x.StartsWith("["))
                        return Path.Combine(
                                _inheritedFrom.GameFolderPath,
                                "libraries",
                                LibrariesResolver.FormatLibraryNameToRelativePath(x.TrimStart('[').TrimEnd(']')))
                            .ToPath();

                    return x.Replace(replaceProcessorArgs)
                        .Replace(replaceValues);
                });

                processor.Outputs = processor.Outputs.ToDictionary(
                    kvp => kvp.Key.Replace(replaceProcessorArgs),
                    kvp => kvp.Value.Replace(replaceProcessorArgs));
            }
        }

        /*
         * Download dependent resources
         */
        ReportProgress(0.25d, "Start downloading dependent resources", TaskStatus.WaitingToRun);
        await libraries.DownloadResourceEntrysAsync(_mirrorDownloadSource, x => {
            ReportProgress(x.ToPercentage().ToPercentage(0.25d, 0.6d), $"Downloading dependent resources：{x.CompletedCount}/{x.TotalCount}",
                TaskStatus.Running);
        });

        /*
         * Write information to version json
         */
        ReportProgress(0.85d, "Write information to version json", TaskStatus.WaitingToRun);
        string forgeLibsFolder = Path.Combine(_inheritedFrom.GameFolderPath,
            "libraries\\net\\minecraftforge\\forge",
            forgeVersion);

        if (isLegacyForgeVersion) {
            var fileName = installProfile.Select("install").GetString("filePath");
            packageArchive.GetEntry(fileName)
                .ExtractTo(Path.Combine(forgeLibsFolder, fileName));
        }


        packageArchive.GetEntry($"maven/net/minecraftforge/forge/{forgeVersion}/forge-{forgeVersion}.jar")?
            .ExtractTo(Path.Combine(forgeLibsFolder, $"forge-{forgeVersion}.jar"));
        packageArchive.GetEntry($"maven/net/minecraftforge/forge/{forgeVersion}/forge-{forgeVersion}-universal.jar")?
            .ExtractTo(Path.Combine(forgeLibsFolder, $"forge-{forgeVersion}-universal.jar"));
        packageArchive.GetEntry("data/client.lzma")?
            .ExtractTo(Path.Combine(forgeLibsFolder, $"forge-{forgeVersion}-clientdata.lzma"));

        if (!string.IsNullOrEmpty(_customId)) {
            versionInfoJson = versionInfoJson
                .SetString("id", _customId);
        }

        var jsonFile = new FileInfo(Path.Combine(
            _inheritedFrom.GameFolderPath,
            "versions",
            versionInfoJson.GetString("id"),
            $"{versionInfoJson.GetString("id")}.json"));

        if (!jsonFile.Directory.Exists) {
            jsonFile.Directory.Create();
        }

        File.WriteAllText(jsonFile.FullName, versionInfoJson.ToString());

        //Legacy version installation completed
        if (isLegacyForgeVersion) {
            ReportProgress(1.0d, "Installation is complete", TaskStatus.Canceled);
            return true;
        }

        /*
         * Running install processor
         */
        int index = 0;
        Dictionary<string, List<string>> _outputs = new();
        Dictionary<string, List<string>> _errorOutputs = new();

        foreach (var processor in highVersionForgeProcessors) {
            var fileName = Path.Combine(
                _inheritedFrom.GameFolderPath,
                "libraries",
                LibrariesResolver.FormatLibraryNameToRelativePath(processor.Jar));

            using var fileArchive = ZipFile.OpenRead(fileName);
            string mainClass = fileArchive.GetEntry("META-INF/MANIFEST.MF")
                .ReadAsString()
                .Split("\r\n".ToCharArray())
                .First(x => x.Contains("Main-Class: "))
                .Replace("Main-Class: ", string.Empty);

            string classPath = string.Join(Path.PathSeparator.ToString(), new List<string>() { fileName }
                .Concat(processor.Classpath.Select(x => Path.Combine(
                    _inheritedFrom.GameFolderPath,
                    "libraries",
                    LibrariesResolver.FormatLibraryNameToRelativePath(x)))));

            var args = new List<string> {
                "-cp",
                classPath.ToPath(),
                mainClass
            };

            args.AddRange(processor.Args);

            using var process = Process.Start(new ProcessStartInfo(_javaPath) {
                Arguments = string.Join(" ", args),
                UseShellExecute = false,
                WorkingDirectory = _inheritedFrom.GameFolderPath,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            });

            var outputs = new List<string>();
            var errorOutputs = new List<string>();

            void AddOutput(string data, bool error = false) {
                if (string.IsNullOrEmpty(data))
                    return;

                outputs.Add(data);
                if (error) errorOutputs.Add(data);
            }

            process.OutputDataReceived += (_, args) => AddOutput(args.Data);
            process.ErrorDataReceived += (_, args) => AddOutput(args.Data, true);

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.WaitForExit();

            _outputs.Add($"{fileName}-{index}", outputs);

            if (errorOutputs.Any()) _errorOutputs.Add($"{fileName}-{index}", errorOutputs);

            index++;

            ReportProgress((index / (double)highVersionForgeProcessors.Count).ToPercentage(0.75, 1.0d),
                $"Running install processor:{index}/{highVersionForgeProcessors.Count}",
                TaskStatus.Running);
        }

        ReportProgress(1.0d, "Installation is complete", TaskStatus.Canceled);
        return true;
    }

    public static async ValueTask<IEnumerable<ForgeInstallEntry>> EnumerableFromVersionAsync(string mcVersion) {
        var packagesUrl = $"https://bmclapi2.bangbang93.com/forge/minecraft/{mcVersion}";
        string json = await packagesUrl.GetStringAsync();

        var entries = json.AsJsonEntry<IEnumerable<ForgeInstallEntry>>();
        entries = entries.OrderByDescending(entry => entry.Build).ToList();
        return entries;
    }
}