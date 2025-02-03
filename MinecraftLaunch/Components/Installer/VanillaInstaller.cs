using Flurl.Http;
using MinecraftLaunch.Base.EventArgs;
using MinecraftLaunch.Base.Interfaces;
using MinecraftLaunch.Base.Models.Game;
using MinecraftLaunch.Base.Models.Network;
using MinecraftLaunch.Components.Downloader;
using MinecraftLaunch.Components.Parser;
using MinecraftLaunch.Extensions;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace MinecraftLaunch.Components.Installer;

public sealed class VanillaInstaller : InstallerBase {
    public VersionManifestEntry Entry { get; init; }
    public override string MinecraftFolder { get; init; }

    public override async Task<MinecraftEntry> InstallAsync(CancellationToken cancellationToken = default) {
        try {
            var dir = await DownloadVersionJsonAsync(cancellationToken);
            var minecraft = ParseMinecraft(dir.Directory, cancellationToken);
            var assetIndex = await DownloadAssetIndexFileAsync(minecraft, cancellationToken);

            await CompleteMinecraftDependenciesAsync(minecraft, cancellationToken);
            ReportProgress(1.0d, "Installation is complete", TaskStatus.Canceled);
            ReportCompleted();

            return minecraft;
        } catch (Exception) {
        }

        return null;
    }

    public static VanillaInstaller Create(string minecraftFolder, VersionManifestEntry entry) {
        return new VanillaInstaller {
            Entry = entry,
            MinecraftFolder = minecraftFolder
        };
    }

    public static async IAsyncEnumerable<VersionManifestEntry> EnumerableMinecraftAsync([EnumeratorCancellation] CancellationToken cancellationToken = default) {
        var url = DownloadMirrorManager.BmclApi
            .TryFindUrl("https://launchermeta.mojang.com/mc/game/version_manifest.json");

        var node = (await url.GetStringAsync(HttpCompletionOption.ResponseContentRead, cancellationToken))
            .AsNode();

        foreach (var entry in node.GetEnumerable("versions").Deserialize(VersionManifestEntryContext.Default.IEnumerableVersionManifestEntry)) {
            cancellationToken.ThrowIfCancellationRequested();
            yield return entry;
        }
    }

    #region Privates

    private async Task<FileInfo> DownloadVersionJsonAsync(CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        ReportProgress(0.15d, "Start downloading the minecraft version json", TaskStatus.WaitingToRun);
        string requestUrl = DownloadMirrorManager.BmclApi.TryFindUrl(Entry.Url);
        var json = await requestUrl.GetStringAsync(HttpCompletionOption.ResponseContentRead, cancellationToken);

        var jsonPath = new FileInfo(Path.Combine(MinecraftFolder, "versions", Entry.Id, $"{Entry.Id}.json"));
        if (!jsonPath.Directory.Exists) {
            jsonPath.Directory.Create();
        }

        await File.WriteAllTextAsync(jsonPath.FullName, json, cancellationToken);
        return jsonPath;
    }

    private async Task<FileInfo> DownloadAssetIndexFileAsync(MinecraftEntry entry, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        var assetIndex = entry.GetAssetIndex();
        var jsonFile = new FileInfo(entry.AssetIndexJsonPath);

        string requestUrl = DownloadMirrorManager.BmclApi.TryFindUrl(assetIndex.Url);
        var json = await requestUrl.GetStringAsync(HttpCompletionOption.ResponseContentRead, cancellationToken);

        if (!jsonFile.Directory.Exists) {
            jsonFile.Directory.Create();
        }

        await File.WriteAllTextAsync(jsonFile.FullName, json, cancellationToken);
        return jsonFile;
    }

    private async Task CompleteMinecraftDependenciesAsync(MinecraftEntry entry, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        ReportProgress(0.45d, "Start verifying and downloading dependent resources", TaskStatus.WaitingToRun);
        var resourceDownloader = new MinecraftResourceDownloader(entry, DownloadMirrorManager.MaxThread);

        resourceDownloader.ProgressChanged += (_, x)
            => ReportProgress(x.ToPercentage().ToPercentage(0.45d, 0.95d),
                    $"Downloading dependent resources：{x.CompletedCount}/{x.TotalCount}",
                        TaskStatus.Running, x.Speed);

        var groupDownloadResult = await resourceDownloader.VerifyAndDownloadDependenciesAsync(cancellationToken: cancellationToken);

        if (groupDownloadResult.Failed.Count > 0)
            throw new InvalidOperationException("Some dependent files encountered errors during download");
    }

    private MinecraftEntry ParseMinecraft(DirectoryInfo dir, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        return MinecraftParser.Parse(dir, null, out var _)
            ?? throw new InvalidOperationException("An incorrect vanilla entry was encountered");
    }

    #endregion
}