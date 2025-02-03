﻿using Flurl.Http;
using MinecraftLaunch.Base.Enums;
using MinecraftLaunch.Base.Models.Game;
using MinecraftLaunch.Base.Models.Network;
using MinecraftLaunch.Components.Downloader;
using MinecraftLaunch.Components.Parser;
using MinecraftLaunch.Extensions;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace MinecraftLaunch.Components.Installer;

public sealed class FabricInstaller : InstallerBase {
    public string CustomId { get; init; }
    public FabricInstallEntry Entry { get; init; }
    public override string MinecraftFolder { get; init; }
    public MinecraftEntry InheritedMinecraft { get; init; }

    public static FabricInstaller Create(string mcFolder, FabricInstallEntry installEntry, string customId = default) {
        return new FabricInstaller {
            CustomId = customId,
            Entry = installEntry,
            MinecraftFolder = mcFolder,
        };
    }

    public static async IAsyncEnumerable<FabricInstallEntry> EnumerableFabricAsync(string mcVersion, [EnumeratorCancellation] CancellationToken cancellationToken = default) {
        string json = await $"https://meta.fabricmc.net/v2/versions/loader/{mcVersion}"
            .GetStringAsync(cancellationToken: cancellationToken);

        var entries = json.Deserialize(FabricInstallEntryContext.Default.IEnumerableFabricInstallEntry)
            .OrderByDescending(x => new Version(x.Loader.Version.Replace(x.Loader.Separator, ".")));

        foreach (var entry in entries) {
            cancellationToken.ThrowIfCancellationRequested();
            yield return entry;
        }
    }

    public override async Task<MinecraftEntry> InstallAsync(CancellationToken cancellationToken = default) {
        ModifiedMinecraftEntry entry = default;
        MinecraftEntry inheritedEntry = default;

        ReportProgress(InstallStep.Started, 0.0d, TaskStatus.WaitingToRun, 1, 1);

        try {
            inheritedEntry = ParseMinecraft(cancellationToken);

            var jsonFile = await DownloadVersionJsonAsync(inheritedEntry, cancellationToken);
            entry = ParseModifiedMinecraft(jsonFile, cancellationToken);
            await CompleteFabricLibrariesAsync(entry, cancellationToken);
        } catch (Exception) {
            ReportProgress(InstallStep.Interrupted, 1.0d, TaskStatus.Canceled, 1, 1);
            ReportCompleted();
        }

        ReportProgress(InstallStep.RanToCompletion, 1.0d, TaskStatus.RanToCompletion, 1, 1);
        ReportCompleted();
        return entry ?? throw new ArgumentNullException(nameof(entry), "Unexpected null reference to variable");
    }

    #region Privates

    private MinecraftEntry ParseMinecraft(CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();
        ReportProgress(InstallStep.ParseMinecraft, 0.10d, TaskStatus.Running, 1, 0);

        if (InheritedMinecraft is not null) {
            return InheritedMinecraft;
        }

        var inheritedMinecraft = new MinecraftParser(MinecraftFolder).GetMinecrafts()
            .FirstOrDefault(x => x.Version.VersionId == Entry.McVersion);

        ReportProgress(InstallStep.ParseMinecraft, 0.15d, TaskStatus.Running, 1, 1);
        return inheritedMinecraft ?? throw new InvalidOperationException("The corresponding version's parent was not found."); ;
    }

    private async Task<FileInfo> DownloadVersionJsonAsync(MinecraftEntry entry, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();
        ReportProgress(InstallStep.DownloadVersionJson, 0.20d, TaskStatus.Running, 1, 0);

        string requestUrl = $"https://meta.fabricmc.net/v2/versions/loader/{Entry.McVersion}/{Entry.BuildVersion}/profile/json";
        requestUrl = DownloadMirrorManager.BmclApi.TryFindUrl(requestUrl);
        var json = await requestUrl.GetStringAsync(HttpCompletionOption.ResponseContentRead, cancellationToken);

        string instanceId = CustomId ??
            json.AsNode().GetString("id") ??
            $"fabric-loader-{Entry.Loader.Version}_{entry.Id}";

        var jsonFile = new FileInfo(Path
            .Combine(MinecraftFolder, "versions", instanceId, $"{instanceId}.json"));

        if (!jsonFile.Directory!.Exists)
            jsonFile.Directory.Create();

        await File.WriteAllTextAsync(jsonFile.FullName,
            JsonNode.Parse(json)!.ToJsonString(new JsonSerializerOptions {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            }), cancellationToken);

        ReportProgress(InstallStep.DownloadVersionJson, 0.45d, TaskStatus.Running, 1, 1);
        return jsonFile;
    }

    private ModifiedMinecraftEntry ParseModifiedMinecraft(FileInfo file, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();
        var entry = MinecraftParser.Parse(file.Directory, null, out var _) as ModifiedMinecraftEntry;

        return entry ?? throw new InvalidOperationException("An incorrect modified entry was encountered");
    }

    private async Task CompleteFabricLibrariesAsync(MinecraftEntry minecraft, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();
        ReportProgress(InstallStep.DownloadLibraries, 0.5d, TaskStatus.Running, 0, 0);

        var resourceDownloader = new MinecraftResourceDownloader(minecraft, DownloadMirrorManager.MaxThread);

        int count = 0;
        resourceDownloader.ProgressChanged += (_, x)
            => ReportProgress(InstallStep.DownloadLibraries, x.ToPercentage().ToPercentage(0.5d, 0.95d),
                TaskStatus.Running, resourceDownloader.TotalCount,
                    Interlocked.Increment(ref count), x.Speed, true);

        var groupDownloadResult = await resourceDownloader.VerifyAndDownloadDependenciesAsync(cancellationToken: cancellationToken);

        if (groupDownloadResult.Failed.Count > 0)
            throw new InvalidOperationException("Some dependent files encountered errors during download");
    }

    #endregion
}