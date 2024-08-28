using Flurl.Http;
using System.Diagnostics;
using MinecraftLaunch.Extensions;
using MinecraftLaunch.Components.Resolver;
using MinecraftLaunch.Classes.Models.Game;
using MinecraftLaunch.Classes.Models.Install;
using MinecraftLaunch.Classes.Models.Download;

namespace MinecraftLaunch.Components.Installer;

public sealed class QuiltInstaller(GameEntry inheritedFrom, QuiltBuildEntry entry, string customId = default, MirrorDownloadSource source = default) : InstallerBase {
    private readonly string _customId = customId;
    private readonly QuiltBuildEntry _quiltBuildEntry = entry;

    public override GameEntry InheritedFrom => inheritedFrom;

    public override async ValueTask<bool> InstallAsync() {
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

        await libraries.DownloadResourceEntrysAsync(source, x => {
            ReportProgress(x.ToPercentage().ToPercentage(0.25d, 0.75d), $"Downloading dependent resources：{x.CompletedCount}/{x.TotalCount}",
                TaskStatus.Running);
        });

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

    public static async ValueTask<IEnumerable<QuiltBuildEntry>> EnumerableFromVersionAsync(string mcVersion) {
        string url = $"https://meta.quiltmc.org/v3/versions/loader/{mcVersion}";
        string json = await url.GetStringAsync();

        var entries = json.AsJsonEntry<IEnumerable<QuiltBuildEntry>>();
        return entries;
    }
}