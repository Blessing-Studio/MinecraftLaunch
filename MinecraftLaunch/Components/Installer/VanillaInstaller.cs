﻿using Flurl.Http;
using MinecraftLaunch.Base.Enums;
using MinecraftLaunch.Base.Models.Game;
using MinecraftLaunch.Base.Models.Network;
using MinecraftLaunch.Components.Downloader;
using MinecraftLaunch.Components.Parser;
using MinecraftLaunch.Extensions;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace MinecraftLaunch.Components.Installer;

public sealed class VanillaInstaller : InstallerBase {
    public VersionManifestEntry Entry { get; init; }
    public override string MinecraftFolder { get; init; }

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

    public override async Task<MinecraftEntry> InstallAsync(CancellationToken cancellationToken = default) {
        MinecraftEntry entry = null;

        ReportProgress(InstallStep.Started, 0.0d, TaskStatus.WaitingToRun, 1, 1);

        try {
            var dir = await DownloadVersionJsonAsync(cancellationToken);
            var minecraft = ParseMinecraft(dir.Directory, cancellationToken);
            var assetIndex = await DownloadAssetIndexFileAsync(minecraft, cancellationToken);

            await CompleteMinecraftDependenciesAsync(minecraft, cancellationToken);

            entry = minecraft;
        } catch (OperationCanceledException) {
            ReportProgress(InstallStep.Interrupted, 1.0d, TaskStatus.Canceled, 1, 1);
            ReportCompleted();
        }

        ReportProgress(InstallStep.RanToCompletion, 1.0d, TaskStatus.RanToCompletion, 1, 1);
        ReportCompleted();
        return entry ?? throw new ArgumentNullException(nameof(entry), "Unexpected null reference to variable"); ;
    }

    #region Privates

    private async Task<FileInfo> DownloadVersionJsonAsync(CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();
        ReportProgress(InstallStep.DownloadVersionJson, 0.15d, TaskStatus.Running, 1, 0);

        string requestUrl = DownloadMirrorManager.BmclApi.TryFindUrl(Entry.Url);
        var json = await requestUrl.GetStringAsync(HttpCompletionOption.ResponseContentRead, cancellationToken);

        var jsonPath = new FileInfo(Path.Combine(MinecraftFolder, "versions", Entry.Id, $"{Entry.Id}.json"));
        if (!jsonPath.Directory.Exists) {
            jsonPath.Directory.Create();
        }

        await File.WriteAllTextAsync(jsonPath.FullName, json, cancellationToken);
        ReportProgress(InstallStep.DownloadVersionJson, 0.3d, TaskStatus.Running, 1, 1);

        return jsonPath;
    }

    private async Task<FileInfo> DownloadAssetIndexFileAsync(MinecraftEntry entry, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();
        ReportProgress(InstallStep.DownloadAssetIndexFile, 0.4d, TaskStatus.Running, 1, 0);

        var assetIndex = entry.GetAssetIndex();
        var jsonFile = new FileInfo(entry.AssetIndexJsonPath);

        string requestUrl = DownloadMirrorManager.BmclApi.TryFindUrl(assetIndex.Url);
        var json = await requestUrl.GetStringAsync(HttpCompletionOption.ResponseContentRead, cancellationToken);

        if (!jsonFile.Directory.Exists) {
            jsonFile.Directory.Create();
        }

        await File.WriteAllTextAsync(jsonFile.FullName, json, cancellationToken);
        ReportProgress(InstallStep.DownloadAssetIndexFile, 0.5d, TaskStatus.Running, 1, 1);

        return jsonFile;
    }

    private async Task CompleteMinecraftDependenciesAsync(MinecraftEntry entry, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        ReportProgress(InstallStep.DownloadLibraries, 0.5d, TaskStatus.Running, 0, 0);
        var resourceDownloader = new MinecraftResourceDownloader(entry, DownloadMirrorManager.MaxThread);
        
        int count = 0;
        resourceDownloader.ProgressChanged += (_, x)
            => ReportProgress(InstallStep.DownloadLibraries, x.ToPercentage().ToPercentage(0.5d, 0.95d),
                TaskStatus.Running, resourceDownloader.TotalCount, 
                    Interlocked.Increment(ref count), x.Speed, true);

        var groupDownloadResult = await resourceDownloader.VerifyAndDownloadDependenciesAsync(cancellationToken: cancellationToken);
        if (groupDownloadResult.Failed.Count > 0)
            throw new InvalidOperationException("Some dependent files encountered errors during download");
    }

    private MinecraftEntry ParseMinecraft(DirectoryInfo dir, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();
        ReportProgress(InstallStep.ParseMinecraft, 0.35d, TaskStatus.Running, 1, 1);

        return MinecraftParser.Parse(dir, null, out var _)
            ?? throw new InvalidOperationException("An incorrect vanilla entry was encountered");
    }

    #endregion
}