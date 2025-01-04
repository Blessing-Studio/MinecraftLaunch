using Flurl.Http;
using MinecraftLaunch.Classes.Models.Download;
using MinecraftLaunch.Classes.Models.Game;
using MinecraftLaunch.Classes.Models.Install;
using MinecraftLaunch.Components.Resolver;
using MinecraftLaunch.Extensions;
using System.Diagnostics;

namespace MinecraftLaunch.Components.Installer;

public sealed class QuiltInstaller : InstallerBase {
    private readonly string _customId;
    private readonly QuiltBuildEntry _quiltBuildEntry;
    private readonly DownloaderConfiguration _configuration;

    public override GameEntry InheritedFrom { get; set; }

    public QuiltInstaller(QuiltBuildEntry entry, string customId = default, DownloaderConfiguration configuration = default) {
        _configuration = configuration;
        _quiltBuildEntry = entry;
        _customId = customId;
    }

    public QuiltInstaller(GameEntry inheritedFrom, QuiltBuildEntry entry, string customId = default, DownloaderConfiguration configuration = default) {
        _configuration = configuration;
        _quiltBuildEntry = entry;
        _customId = customId;
        InheritedFrom = inheritedFrom;
    }

    public override async Task<bool> InstallAsync(CancellationToken cancellation = default) {
        /*
         * Parse build
         */
        ReportProgress(0.0d, "Start parse build", TaskStatus.Created);
        string url = $"https://meta.quiltmc.org/v3/versions/loader/{_quiltBuildEntry.McVersion}/{_quiltBuildEntry.BuildVersion}/profile/json";
        var versionInfoNode = (await url.GetStringAsync())
            .AsNode();

        var libraries = LibrariesResolver.GetLibrariesFromJsonArray(versionInfoNode
                .GetEnumerable("libraries"),
            InheritedFrom.GameFolderPath);

        /*
         * Download dependent resources
         */
        ReportProgress(0.25d, "Start downloading dependent resources", TaskStatus.WaitingToRun);
        foreach (var library in libraries) {
            Debug.WriteLine(library.Url);
        }

        await libraries.DownloadResourceEntrysAsync(_configuration, x => {
            ReportProgress(x.ToPercentage().ToPercentage(0.25d, 0.75d), $"Downloading dependent resources：{x.CompletedCount}/{x.TotalCount}",
                TaskStatus.Running);
        }, cancellation);

        /*
         * Write information to version json
         */
        ReportProgress(0.85d, "Write information to version json", TaskStatus.WaitingToRun);
        if (!string.IsNullOrEmpty(_customId)) {
            versionInfoNode = versionInfoNode.SetString("id", _customId);
        }

        var id = versionInfoNode.GetString("id");
        var jsonFile = new FileInfo(Path.Combine(InheritedFrom.GameFolderPath,
            "versions", id, $"{id}.json"));

        if (!jsonFile.Directory.Exists) {
            jsonFile.Directory.Create();
        }

        File.WriteAllText(jsonFile.FullName, versionInfoNode.ToString());
        ReportProgress(1.0d, "Installation is complete", TaskStatus.Canceled);
        return true;
    }

    public static async ValueTask<IEnumerable<QuiltBuildEntry>> EnumerableFromVersionAsync(string mcVersion, CancellationToken cancellation = default) {
        string url = $"https://meta.quiltmc.org/v3/versions/loader/{mcVersion}";
        string json = await url.GetStringAsync(cancellationToken: cancellation);

        var entries = json.AsJsonEntry<IEnumerable<QuiltBuildEntry>>();
        return entries;
    }
}