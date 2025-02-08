﻿using Flurl.Http;
using MinecraftLaunch.Base.Interfaces;
using MinecraftLaunch.Base.Models.Game;
using MinecraftLaunch.Base.Models.Network;
using MinecraftLaunch.Components.Downloader;
using MinecraftLaunch.Components.Parser;
using MinecraftLaunch.Extensions;
using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace MinecraftLaunch.Components.Installer;

/// <summary>
/// Forge（Neo）通用安装器
/// </summary>
public sealed class ForgeInstaller : InstallerBase {
    public string CustomId { get; init; }
    public string JavaPath { get; init; }
    public ForgeInstallEntry Entry { get; init; }
    public override string MinecraftFolder { get; init; }
    public MinecraftEntry InheritedMinecraft { get; init; }

    public static ForgeInstaller Create(string folder, string javaPath, ForgeInstallEntry installEntry, string customId = default) {
        return new ForgeInstaller {
            CustomId = customId,
            JavaPath = javaPath,
            Entry = installEntry,
            MinecraftFolder = folder
        };
    }

    public override async Task<MinecraftEntry> InstallAsync(CancellationToken cancellationToken = default) {
        FileInfo forgePackageFile = default;
        MinecraftEntry inheritedEntry = default;
        ModifiedMinecraftEntry entry = default;

        try {
            inheritedEntry = ParseMinecraft(cancellationToken);
            forgePackageFile = await DownloadForgePackageAsync(cancellationToken);

            var (package, installProfile, isLegacy) = ParseForgePackage(forgePackageFile.FullName, cancellationToken);
            var forgeClientFile = await WriteVersionJsonAndSomeDependenciesAsync(isLegacy, installProfile, package, cancellationToken);

            entry = ParseModifiedMinecraft(forgeClientFile, cancellationToken);
            await CompleteForgeDependenciesAsync(isLegacy, installProfile, entry, cancellationToken);

            if (!isLegacy) {
                await RunInstallProcessorAsync(forgePackageFile.FullName, installProfile, entry, cancellationToken);
            } else {
                //ReportProgress(1.0d, "Installation is complete", TaskStatus.RanToCompletion);
                ReportCompleted();
            }
        } catch (Exception) {

        } finally {
            forgePackageFile?.Delete();
        }

        return entry ?? throw new ArgumentNullException(nameof(entry), "Unexpected null reference to variable");
    }

    public static async IAsyncEnumerable<ForgeInstallEntry> EnumerableForgeAsync(string mcVersion, bool isNeoforge = false, [EnumeratorCancellation] CancellationToken cancellationToken = default) {
        var packagesUrl = isNeoforge
            ? $"https://bmclapi2.bangbang93.com/neoforge/list/{mcVersion}"
            : $"https://bmclapi2.bangbang93.com/forge/minecraft/{mcVersion}";

        string json = await packagesUrl.GetStringAsync(cancellationToken: cancellationToken);
        var entries = json.Deserialize(ForgeInstallEntryContext.Default.IEnumerableForgeInstallEntry)
            .OrderByDescending(entry => entry.Build);

        foreach (var package in entries) {
            cancellationToken.ThrowIfCancellationRequested();

            package.IsNeoforge = isNeoforge;
            yield return package;
        }
    }

    #region Privates

    private MinecraftEntry ParseMinecraft(CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();
        //ReportProgress(0.15d, "Start parse minecraft", TaskStatus.Created);

        if (InheritedMinecraft is not null) {
            return InheritedMinecraft;
        }

        var inheritedMinecraft = new MinecraftParser(MinecraftFolder).GetMinecrafts()
            .FirstOrDefault(x => x.Version.VersionId == Entry.McVersion);

        return inheritedMinecraft ?? throw new InvalidOperationException("The corresponding version's parent was not found."); ;
    }

    private async Task<FileInfo> DownloadForgePackageAsync(CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();
        //ReportProgress(0.30d, "Downloading forge package", TaskStatus.Running);

        string baseUrl = Entry.IsNeoforge
            ? $"https://bmclapi2.bangbang93.com/neoforge/version/{Entry.ForgeVersion}/download/installer.jar"
            : $"https://bmclapi2.bangbang93.com/forge/download?mcversion={Entry.McVersion}&version={Entry.ForgeVersion}&category=installer&format=jar";

        //    string baseUrl = Entry.IsNeoforge
        //? $"https://maven.neoforged.net/releases/net/neoforged/forge/{Entry.McVersion}-{Entry.ForgeVersion}/forge-{Entry.McVersion}-{Entry.ForgeVersion}-installer.jar"
        //: $"https://files.minecraftforge.net/maven/net/minecraftforge/forge/{Entry.McVersion}-{Entry.ForgeVersion}/forge-{Entry.McVersion}-{Entry.ForgeVersion}-installer.jar";

        string branchParam = string.IsNullOrEmpty(Entry.Branch) ? string.Empty : $"&branch={Entry.Branch}";
        string packageUrl = $"{baseUrl}{branchParam}";
        string fileName = Entry.IsNeoforge
            ? $"neoforge-{Entry.ForgeVersion}-installer.jar"
            : $"forge-{Entry.McVersion}-{Entry.ForgeVersion}" +
                $"{(string.IsNullOrEmpty(Entry.Branch) ? string.Empty : $"-{Entry.Branch}")}" +
                $"-installer.jar";

        var packageFile = new FileInfo(Path.Combine(MinecraftFolder, fileName));
        var downloadRequest = new DownloadRequest(packageUrl,
            packageFile.FullName);

        downloadRequest.BytesDownloaded += Console.WriteLine;
        await new FileDownloader(DownloadMirrorManager.MaxThread)
            .DownloadFileAsync(downloadRequest, cancellationToken);

        return packageFile;
    }

    private (ZipArchive package, JsonNode installProfile, bool isLegacy) ParseForgePackage(string packageFilePath, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();
        //ReportProgress(0.45d, "Parse forge package", TaskStatus.Running);

        var packageArchive = ZipFile.OpenRead(packageFilePath);
        var installProfileNode = packageArchive
            .GetEntry("install_profile.json")
            ?.ReadAsString()
            .AsNode()
            ?? throw new Exception("Failed to parse install_profile.json");

        bool isLegacyForgeVersion = installProfileNode.Select("install") != null;
        return (packageArchive, installProfileNode, isLegacyForgeVersion);
    }

    private async Task<FileInfo> WriteVersionJsonAndSomeDependenciesAsync(bool isLegacyForgeVersion, JsonNode installProfile, ZipArchive packageArchive, CancellationToken cancellationToken) {
        string forgeVersion = $"{Entry.McVersion}-{Entry.ForgeVersion}";
        string forgeLibsFolder = Path.Combine(MinecraftFolder, "libraries\\net\\minecraftforge\\forge", forgeVersion);

        if (isLegacyForgeVersion) {
            var universalFilePath = installProfile.Select("install").GetString("filePath")
                ?? throw new InvalidDataException("Unable to resolve location of universal file in archive");

            var universalFileEntry = packageArchive.GetEntry(universalFilePath)
                ?? throw new FileNotFoundException("The universal file was not found in the archive");

            universalFileEntry.ExtractTo(Path.Combine(forgeLibsFolder, universalFileEntry.Name.Replace("-universal", string.Empty)));
        }

        if (packageArchive.GetEntry($"maven/net/minecraftforge/forge/{forgeVersion}/") != null)
            foreach (var entry in packageArchive.Entries.Where(x => !x.FullName.EndsWith('/') && x.FullName.StartsWith($"maven/net/minecraftforge/forge/{forgeVersion}")))
                entry.ExtractTo(Path.Combine(forgeLibsFolder, entry.Name));

        packageArchive.GetEntry("data/client.lzma")?.ExtractTo(Path.Combine(forgeLibsFolder, $"forge-{forgeVersion}-clientdata.lzma"));

        string jsonContent = (isLegacyForgeVersion
            ? installProfile.Select("versionInfo")!.ToString()
            : packageArchive.GetEntry("version.json")?.ReadAsString())
            ?? throw new Exception("Failed to read version.json");
        var jsonNode = JsonNode.Parse(jsonContent);

        string entryId = CustomId ?? $"{Entry.McVersion}-{(Entry.IsNeoforge ? "neoforge" : "forge")}-{Entry.ForgeVersion}";
        var jsonFile = new FileInfo(Path.Combine(MinecraftFolder, "versions", entryId, $"{entryId}.json"));

        if (!jsonFile.Directory!.Exists)
            jsonFile.Directory.Create();

        jsonNode!["id"] = entryId;
        await File.WriteAllTextAsync(jsonFile.FullName, jsonNode.ToJsonString(), cancellationToken);

        return jsonFile;
    }

    private ModifiedMinecraftEntry ParseModifiedMinecraft(FileInfo file, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();
        var entry = MinecraftParser.Parse(file.Directory, null, out var _) as ModifiedMinecraftEntry;

        return entry ?? throw new InvalidOperationException("An incorrect modified entry was encountered");
    }

    private async Task CompleteForgeDependenciesAsync(bool isLegacyForgeVersion, JsonNode installProfile, MinecraftEntry minecraft, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        string forgeVersion = $"{Entry.McVersion}-{Entry.ForgeVersion}";
        var dependencies = new List<MinecraftLibrary>();

        var libraries = minecraft.GetRequiredLibraries().Libraries.ToList();
        foreach (var lib in libraries.Where(x => x.MavenName.Equals($"net.minecraftforge:forge:{forgeVersion}")
            || x.MavenName.Equals($"net.minecraftforge:forge:{forgeVersion}:client") || x is not IDownloadDependency).ToArray())
            libraries.Remove(lib);

        dependencies.AddRange(libraries);

        if (!isLegacyForgeVersion) {
            var processorLibraries = installProfile.Select("libraries")
                .Deserialize(LibraryEntryContext.Default.IEnumerableLibraryEntry)?
                .Select(lib => MinecraftLibrary.ParseJsonNode(lib, MinecraftFolder))
                ?? throw new InvalidDataException();

            foreach (var item in processorLibraries)
                if (!dependencies.Contains(item))
                    dependencies.Add(item);
        }

        var groupDownloadRequest = new GroupDownloadRequest(dependencies.OfType<IDownloadDependency>()
            .Select(x => new DownloadRequest(DownloadMirrorManager.BmclApi.TryFindUrl(x.Url), x.FullPath)));

        int count = 0;
        double speed = 0;
        groupDownloadRequest.DownloadSpeedChanged += x => speed = x;
        //groupDownloadRequest.SingleRequestCompleted += (_, x)
        //    => ReportProgress(((double)count * (double)dependencies.Count).ToPercentage(0.45d, 0.70d),
        //            $"Downloading dependent resources：{Interlocked.Increment(ref count)}/{dependencies.Count}",
        //                TaskStatus.Running, speed);

        var groupDownloadResult = await new FileDownloader(DownloadMirrorManager.MaxThread)
            .DownloadFilesAsync(groupDownloadRequest, cancellationToken);

        if (groupDownloadResult.Failed.Count > 0)
            throw new InvalidOperationException("Some dependent files encountered errors during download");

    }

    private async Task RunInstallProcessorAsync(string packageFilePath, JsonNode installProfile, MinecraftEntry entry, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        Dictionary<string, Dictionary<string, string>> forgeDataDictionary = installProfile.Select("data")
            .Deserialize(ForgeInstallerContext.Default.DictionaryStringDictionaryStringString)
            ?? throw new Exception("Failed to parse install profile data");

        string forgeVersion = $"{Entry.McVersion}-{Entry.ForgeVersion}";

        if (forgeDataDictionary.TryGetValue("BINPATCH", out Dictionary<string, string> value)) {
            value["client"] = $"[net.minecraftforge:forge:{forgeVersion}:clientdata@lzma]";
            value["server"] = $"[net.minecraftforge:forge:{forgeVersion}:serverdata@lzma]";
        }

        var replaceValues = new Dictionary<string, string> {
            { "{SIDE}", "client" },
            { "{MINECRAFT_JAR}", entry.ClientJarPath },
            { "{MINECRAFT_VERSION}", Entry.McVersion },
            { "{ROOT}", MinecraftFolder.ToPath() },
            { "{INSTALLER}", packageFilePath.ToPath() },
            { "{LIBRARY_DIR}", Path.Combine(MinecraftFolder, "libraries").ToPath() }
        };

        var replaceProcessorArgs = forgeDataDictionary.ToDictionary(
            kvp => $"{{{kvp.Key}}}", kvp => {
                var value = kvp.Value["client"];
                if (!value.StartsWith('[')) return value;

                return Path.Combine(MinecraftFolder, "libraries", value.TrimStart('[').TrimEnd(']')
                    .FormatLibraryNameToRelativePath())
                    .ToPath();
            });

        var forgeProcessors = installProfile.Select("processors")?
            .Deserialize(ForgeInstallerContext.Default.IEnumerableForgeProcessorData)?
            .Where(x => !(x.Sides.Count == 1 && x.Sides.Contains("server")))
            .ToArray()
            ?? throw new InvalidDataException("Unable to parse Forge Processors");

        int count = 0;
        int totalCount = forgeProcessors.Length;
        foreach (var processor in forgeProcessors) {
            cancellationToken.ThrowIfCancellationRequested();

            processor.Args = processor.Args.Select(x => {
                if (x.StartsWith("["))
                    return Path.Combine(MinecraftFolder, "libraries", x.TrimStart('[').TrimEnd(']').FormatLibraryNameToRelativePath())
                        .ToPath();

                return x.ReplaceFromDictionary(replaceProcessorArgs)
                    .ReplaceFromDictionary(replaceValues);
            });

            processor.Outputs = processor.Outputs.ToDictionary(
                kvp => kvp.Key.ReplaceFromDictionary(replaceProcessorArgs),
                kvp => kvp.Value.ReplaceFromDictionary(replaceProcessorArgs));

            var fileName = Path.Combine(MinecraftFolder, "libraries", processor.Jar.FormatLibraryNameToRelativePath());

            using var fileArchive = ZipFile.OpenRead(fileName);
            string mainClass = fileArchive.GetEntry("META-INF/MANIFEST.MF")?
                .ReadAsString()
                .Split("\r\n".ToCharArray())
                .FirstOrDefault(x => x.Contains("Main-Class: "))
                ?.Replace("Main-Class: ", string.Empty)
                ?? throw new InvalidDataException("Unable to find MainClass for Processor");

            string classPath = string.Join(Path.PathSeparator.ToString(), new List<string>() { fileName }
                .Concat(processor.Classpath.Select(x => Path.Combine(MinecraftFolder, "libraries", x.FormatLibraryNameToRelativePath()))));

            var args = new List<string> {
                "-cp",
                classPath.ToPath(),
                mainClass
            };

            args.AddRange(processor.Args);

            using var process = Process.Start(new ProcessStartInfo(JavaPath) {
                Arguments = string.Join(" ", args),
                UseShellExecute = false,
                WorkingDirectory = MinecraftFolder,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            }) ?? throw new Exception("Failed to start Java");

            List<string> _errorOutputs = [];

            process.ErrorDataReceived += (_, args) => {
                if (args.Data is string data && !string.IsNullOrEmpty(data))
                    _errorOutputs.Add(args.Data);
            };

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync(cancellationToken);

            //ReportProgress(((double)count * (double)totalCount).ToPercentage(0.45d, 0.95d),
            //    $"Running install processor:{Interlocked.Increment(ref count)}/{totalCount}",
            //        TaskStatus.Running);
        }
    }

    #endregion
}

public record ForgeProcessorData {
    [JsonPropertyName("jar")] public string Jar { get; set; } = null!;
    [JsonPropertyName("sides")] public List<string> Sides { get; set; } = [];
    [JsonPropertyName("args")] public IEnumerable<string> Args { get; set; } = null!;
    [JsonPropertyName("classpath")] public IEnumerable<string> Classpath { get; set; } = null!;
    [JsonPropertyName("outputs")] public Dictionary<string, string> Outputs { get; set; } = [];
}

[JsonSerializable(typeof(IEnumerable<ForgeProcessorData>))]
[JsonSerializable(typeof(Dictionary<string, Dictionary<string, string>>))]
internal sealed partial class ForgeInstallerContext : JsonSerializerContext;
