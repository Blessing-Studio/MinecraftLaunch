using Flurl.Http;
using MinecraftLaunch.Classes.Models.Download;
using MinecraftLaunch.Classes.Models.Game;
using MinecraftLaunch.Classes.Models.Install;
using MinecraftLaunch.Components.Resolver;
using MinecraftLaunch.Extensions;
using System.Diagnostics;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MinecraftLaunch.Components.Installer;

public sealed class OptifineInstaller(
    GameEntry inheritedFrom,
    OptiFineInstallEntity installEntry,
    string javaPath, string customId = default,
    DownloaderConfiguration configuration = default) : InstallerBase {
    private readonly string _customId = customId;
    private readonly string _javaPath = javaPath;
    private readonly OptiFineInstallEntity _installEntry = installEntry;
    private readonly DownloaderConfiguration _configuration = configuration;

    public override GameEntry InheritedFrom => inheritedFrom;

    public override async Task<bool> InstallAsync(CancellationToken cancellation = default) {
        /*
         * Download Optifine installation package
         */
        string downloadUrl = $"https://bmclapi2.bangbang93.com/optifine/{_installEntry.McVersion}/{_installEntry.Type}/{_installEntry.Patch}";
        string packagePath = Path.Combine(Path.GetTempPath(),
            Path.GetFileName(downloadUrl));

        var request = downloadUrl.ToDownloadRequest(packagePath.ToFileInfo());
        await request.DownloadAsync(x => {
            ReportProgress(x.ToPercentage(0.0d, 0.15d),
                "Downloading Optifine installation package",
                TaskStatus.Running);
        }, cancellation);

        /*
         * Parse package
         */
        ReportProgress(0.15d, "Start parse package", TaskStatus.Created);

        var packageArchive = ZipFile.OpenRead(request.FileInfo.FullName);
        var launchwrapper = packageArchive.GetEntry("launchwrapper-of.txt")?.ReadAsString() ?? "1.12";

        var changelogLine = packageArchive.GetEntry("changelog.txt")?.ReadAsString().Split("\r\n")[0];
        var rawPatch = changelogLine[9..].Split('_');

        var packageMcVersion = rawPatch[0];
        var packagePatch = changelogLine[9..][(packageMcVersion.Length + 1)..];

        /*
         * Write information to version json
         */
        ReportProgress(0.85d, "Write information to version json", TaskStatus.WaitingToRun);

        var time = DateTime.Now.ToString("s");

        var jsonEntity = new {
            id = _customId ?? $"{packageMcVersion}-OptiFine-{packagePatch}",
            inheritsFrom = packageMcVersion,
            time,
            releaseTime = time,
            type = "release",
            libraries = new LibraryJsonEntry[]
            {
                new () { Name = $"optifine:Optifine:{packageMcVersion}_{packagePatch}" },
                new () { Name = launchwrapper.Equals("1.12") ? "net.minecraft:launchwrapper:1.12" : $"optifine:launchwrapper-of:{launchwrapper}" }
            },
            mainClass = "net.minecraft.launchwrapper.Launch",
            minecraftArguments = "  --tweakClass optifine.OptiFineTweaker"
        };

        var jarFilePath = Path.Combine(InheritedFrom.GameFolderPath, "versions", jsonEntity.id, $"{jsonEntity.id}.jar");
        var jsonFilePath = Path.Combine(InheritedFrom.GameFolderPath, "versions", jsonEntity.id, $"{jsonEntity.id}.json");
        var launchwrapperFile = Path.Combine(
                InheritedFrom.GameFolderPath,
                "libraries",
                LibrariesResolver.FormatLibraryNameToRelativePath(jsonEntity.libraries[1].Name));

        var optifineLibraryPath = Path.Combine(
                InheritedFrom.GameFolderPath,
                "libraries",
                LibrariesResolver.FormatLibraryNameToRelativePath(jsonEntity.libraries[0].Name));

        foreach (var path in new string[] { jsonFilePath, launchwrapperFile, optifineLibraryPath }) {
            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        }

        File.WriteAllText(Path.Combine(jsonFilePath),
            JsonSerializer.Serialize(jsonEntity, new JsonSerializerOptions {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            }));

        packageArchive.GetEntry($"launchwrapper-of-{launchwrapper}.jar").ExtractToFile(launchwrapperFile, true);
        File.Copy(InheritedFrom.JarPath, jarFilePath, true);

        /*
         * Running install processor
         */
        using var process = Process.Start(new ProcessStartInfo(_javaPath) {
            UseShellExecute = false,
            WorkingDirectory = InheritedFrom.GameFolderPath,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            Arguments = string.Join(" ", [
                "-cp",
                packagePath.ToPath(),
                "optifine.Patcher",
                InheritedFrom.JarPath.ToPath(),
                packagePath.ToPath(),
                optifineLibraryPath.ToPath()
                ])
        });

        process.BeginErrorReadLine();
        process.BeginOutputReadLine();

        process.WaitForExit();
        ReportProgress(1.0d, "Installation is complete", TaskStatus.Canceled);
        return true;
    }

    public static async ValueTask<IEnumerable<OptiFineInstallEntity>> EnumerableFromVersionAsync(string mcVersion, CancellationToken cancellation = default) {
        string url = $"https://bmclapi2.bangbang93.com/optifine/{mcVersion}";
        using var responseMessage = await url.GetAsync(cancellationToken: cancellation);
        responseMessage.ResponseMessage.EnsureSuccessStatusCode();

        await using var responseStream = await responseMessage.GetStreamAsync();
        var list = await JsonSerializer.DeserializeAsync<IEnumerable<OptiFineInstallEntity>>(responseStream, cancellationToken: cancellation);

        return list.OrderBy(x => x.Type)
            .ThenBy(x => x.Patch.StartsWith("pre"))
            .ThenBy(x => x.Patch)
            .Reverse();
    }
}