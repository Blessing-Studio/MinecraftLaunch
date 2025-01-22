using Flurl.Http;
using MinecraftLaunch.Classes.Interfaces;
using MinecraftLaunch.Classes.Models.Download;
using MinecraftLaunch.Classes.Models.Game;
using MinecraftLaunch.Classes.Models.Install;
using MinecraftLaunch.Components.Checker;
using MinecraftLaunch.Extensions;
using System.Text.Json;

namespace MinecraftLaunch.Components.Installer;

/// <summary>
/// 原版核心安装器
/// </summary>
public sealed class VanillaInstaller(IGameResolver gameFoloder, string gameId, DownloaderConfiguration configuration = default) : InstallerBase {
    private readonly string _gameId = gameId;
    private readonly IGameResolver _gameResolver = gameFoloder;
    private readonly DownloaderConfiguration _configuration = configuration;

    public override GameEntry InheritedFrom { get; set; }

    public override async Task<bool> InstallAsync(CancellationToken cancellation = default) {
        /*
         * Check if the specified id exists
         */
        ReportProgress(0.0d, "Check if the specified id exists", TaskStatus.Created);
        var cache = await EnumerableGameCoreAsync(cancellation);
        if (cache is null || string.IsNullOrEmpty(_gameId)) {
            return false;
        }

        /*
         * Download game core json
         */
        ReportProgress(0.15d, "Start downloading the game core json", TaskStatus.WaitingToRun);
        var coreInfo = cache.SingleOrDefault(x => x.Id == _gameId);
        if (coreInfo is null) {
            return false;
        }

        var versionJsonFile = Path.Combine(_gameResolver.Root.FullName, "versions", _gameId,
            $"{_gameId}.json").ToFileInfo();

        if (versionJsonFile.Directory is { Exists: false }) {
            versionJsonFile.Directory.Create();
        }

        await File.WriteAllTextAsync(versionJsonFile.FullName,
            await coreInfo.Url.GetStringAsync(cancellationToken: cancellation));

        /*
         * Download dependent resources
         */
        ReportProgress(0.45d, "Start downloading dependent resources", TaskStatus.WaitingToRun);
        ResourceChecker resourceChecker = new(_gameResolver.GetGameEntity(_gameId));
        var hasMissResource = await resourceChecker.CheckAsync();
        if (!hasMissResource) {
            await resourceChecker.MissingResources.DownloadResourceEntrysAsync(_configuration, x => {
                ReportProgress(x.ToPercentage().ToPercentage(0.45d, 0.95d),
                    $"Downloading dependent resources：{x.CompletedCount}/{x.TotalCount}",
                    TaskStatus.Running, x.Speed);
            }, cancellation);
        }

        InheritedFrom = _gameResolver.GetGameEntity(_gameId);
        ReportProgress(1.0d, "Installation is complete", TaskStatus.Canceled);
        ReportCompleted();
        return true;
    }

    public static async ValueTask<IEnumerable<VersionManifestEntry>> EnumerableGameCoreAsync(CancellationToken cancellation = default) {
        string url = MirrorDownloadManager.IsUseMirrorDownloadSource
            ? MirrorDownloadManager.Bmcl.VersionManifestUrl
            : "https://launchermeta.mojang.com/mc/game/version_manifest.json";

        var node = (await url.GetStringAsync(cancellationToken: cancellation))
            .AsNode();

        return node.GetEnumerable("versions").Deserialize<IEnumerable<VersionManifestEntry>>();
    }
}