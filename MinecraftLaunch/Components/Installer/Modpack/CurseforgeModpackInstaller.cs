using MinecraftLaunch.Base.Enums;
using MinecraftLaunch.Base.Interfaces;
using MinecraftLaunch.Base.Models.Game;
using MinecraftLaunch.Base.Models.Network;
using MinecraftLaunch.Components.Downloader;
using MinecraftLaunch.Components.Provider;
using MinecraftLaunch.Extensions;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace MinecraftLaunch.Components.Installer.Modpack;

public sealed class CurseforgeModpackInstaller : InstallerBase {
    public string ModpackPath { get; init; }
    public MinecraftEntry Minecraft { get; init; }
    public override string MinecraftFolder { get; init; }
    public CurseforgeModpackInstallEntry Entry { get; init; }

    [Obsolete("Implemented processing method")]
    public List<long> FaildParseModProjectId { get; set; } = [];

    public static CurseforgeModpackInstaller Create(string mcFolder, string modpackPath, CurseforgeModpackInstallEntry installEntry, MinecraftEntry entry) {
        return new CurseforgeModpackInstaller {
            Minecraft = entry,
            Entry = installEntry,
            ModpackPath = modpackPath,
            MinecraftFolder = mcFolder
        };
    }

    public static CurseforgeModpackInstallEntry ParseModpackInstallEntry(string modpackPath) {
        using var zipArchive = ZipFile.OpenRead(modpackPath);
        var json = zipArchive?.GetEntry("manifest.json")?.ReadAsString()
            ?? throw new ArgumentException("Not found manifest.json");

        var entry = json.Deserialize(CurseforgeModpackInstallEntryContext.Default.CurseforgeModpackInstallEntry)
            ?? throw new InvalidOperationException("Failed to parse manifest.json");

        return entry;
    }

    public static async IAsyncEnumerable<IInstallEntry> ParseModLoaderEntryByManifestAsync(CurseforgeModpackInstallEntry entry, [EnumeratorCancellation] CancellationToken cancellationToken = default) {
        foreach (var loader in entry.Minecraft.ModLoaders) {
            cancellationToken.ThrowIfCancellationRequested();

            (bool isPrimary, string id) = (loader.GetBool("primary"), loader.GetString("id"));

            var idDatas = id.Split('-');

            var loaderVersion = idDatas.Last();
            var loaderType = idDatas.First() switch {
                "forge" => ModLoaderType.Forge,
                "fabric" => ModLoaderType.Fabric,
                "neoforge" => ModLoaderType.NeoForge,
                _ => throw new NotSupportedException("Unsupported installer type")
            };

            IInstallEntry installEntry = loaderType switch {
                ModLoaderType.Forge => await ForgeInstaller.EnumerableForgeAsync(entry.McVersion, cancellationToken: cancellationToken)
                    .FirstOrDefaultAsync(x => x.ForgeVersion.Equals(loaderVersion), cancellationToken),

                ModLoaderType.Fabric => await FabricInstaller.EnumerableFabricAsync(entry.McVersion, cancellationToken: cancellationToken)
                    .FirstOrDefaultAsync(x => x.BuildVersion.Equals(loaderVersion), cancellationToken),

                ModLoaderType.NeoForge => await ForgeInstaller.EnumerableForgeAsync(entry.McVersion, true, cancellationToken)
                    .FirstOrDefaultAsync(x => x.ForgeVersion.Equals(loaderVersion), cancellationToken),

                _ => throw new NotImplementedException()
            };

            yield return installEntry ?? throw new InvalidOperationException();
        }
    }

    public override async Task<MinecraftEntry> InstallAsync(CancellationToken cancellationToken = default) {
        ReportProgress(InstallStep.Started, 0.0d, TaskStatus.WaitingToRun, 1, 1);

        try {
            var downloadUrls = await ParseModFilesAsync(cancellationToken)
                .ToListAsync(cancellationToken);

            await DownloadModsAsync(downloadUrls, cancellationToken);
            await ExtractModpackAsync(cancellationToken);
        } catch (Exception) {
        }

        ReportProgress(InstallStep.RanToCompletion, 1.0d, TaskStatus.RanToCompletion, 1, 1);
        ReportCompleted();
        return Minecraft;
    }

    #region Privates

    private void ParseMinecraft(CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();
        ReportProgress(InstallStep.ParseMinecraft, 0.05d, TaskStatus.Running, 1, 0);

        if (Minecraft is not null && Minecraft is ModifiedMinecraftEntry && Minecraft.Version.VersionId.Equals(Entry.McVersion)) {
            ReportProgress(InstallStep.ParseMinecraft, 0.1d, TaskStatus.Running, 1, 1);
            return;
        }

        throw new NotSupportedException("Your entry is incorrect or does not exist");
    }

    private async IAsyncEnumerable<string> ParseModFilesAsync([EnumeratorCancellation] CancellationToken cancellationToken) {
        int count = 0;
        int totalCount = Entry.ModFiles.Count();
        List<Task> requestTasks = [];
        List<string> downloadUrls = [];
        SemaphoreSlim semaphoreSlim = new(256, 256);

        ReportProgress(InstallStep.ParseDownloadUrls, 0.1d, TaskStatus.Running, totalCount, count);

        foreach (var modpackFile in Entry.ModFiles) {
            string downloadUrl = string.Empty;

            requestTasks.Add(Task.Run(async () => {
                await semaphoreSlim.WaitAsync(cancellationToken);
                if (modpackFile.IsRequired) {
                    try {
                        downloadUrl = await CurseforgeProvider.GetModDownloadUrlAsync(modpackFile.ProjectId, modpackFile.FileId, cancellationToken);
                        downloadUrl = downloadUrl.Replace("https://edge.forgecdn.net", "https://mediafiles.forgecdn.net");
                    } catch (InvalidModpackFileException) {
                        var entry = await CurseforgeProvider.GetModFileEntryAsync(modpackFile.ProjectId, modpackFile.FileId, cancellationToken);
                        downloadUrl = await CurseforgeProvider.TestDownloadUrlAsync(modpackFile.FileId, entry.GetString("fileName"));
                    }

                } else return;

                lock (requestTasks) {
                    var progress = (double)Interlocked.Increment(ref count) / (double)totalCount;
                    ReportProgress(InstallStep.ParseDownloadUrls, progress.ToPercentage(0.1d, 0.5d),
                        TaskStatus.Running, totalCount, count);

                    downloadUrls.Add(downloadUrl);
                }

                semaphoreSlim.Release();
            }, cancellationToken));
        }

        await Task.WhenAll(requestTasks);
        foreach (var downloadUrl in downloadUrls) {
            yield return downloadUrl;
        }
    }

    //private async IAsyncEnumerable<string> RedirectInvalidModsAsync([EnumeratorCancellation] CancellationToken cancellationToken) {
    //    throw null;
    //}

    private async Task DownloadModsAsync(IEnumerable<string> asyncUrls, CancellationToken cancellationToken) {
        double speed = 0;
        int currentCount = 0;
        List<Task> downloadTasks = [];
        var urls = asyncUrls.ToList();

        var modsPath = new DirectoryInfo(Path.Combine(Minecraft.ToWorkingPath(true), "mods"));
        if (!modsPath.Exists)
            modsPath.Create();

        var groupRequest = new GroupDownloadRequest(urls
            .Select(x => new DownloadRequest(x, Path.Combine(modsPath.FullName,
                Path.GetFileName(x)))));

        groupRequest.DownloadSpeedChanged += arg => speed = arg;
        groupRequest.SingleRequestCompleted += (request, result) => {
            var progress = (double)Interlocked.Increment(ref currentCount) / urls.Count;
            ReportProgress(InstallStep.DownloadMods, progress.ToPercentage(0.5d, 0.85d),
                TaskStatus.Running, urls.Count, currentCount, speed, true);
        };

        ReportProgress(InstallStep.DownloadMods, 0.5d, TaskStatus.Running,
            urls.Count, 0, 0, true);

        await new FileDownloader().DownloadFilesAsync(groupRequest, cancellationToken);
    }

    private async Task ExtractModpackAsync(CancellationToken cancellationToken) {
        var zipArchive = ZipFile.OpenRead(ModpackPath);
        var entries = zipArchive?.Entries;
        ReportProgress(InstallStep.ExtractModpack, 0.85d, TaskStatus.Running, entries.Count, 0);

        int count = 0;
        var tasks = entries.Select(x => Task.Run(() => {
            lock (zipArchive) {
                ReportProgress(InstallStep.ExtractModpack,
                    ((double)Interlocked.Increment(ref count) / (double)entries.Count).ToPercentage(0.85d, 1.0d),
                    TaskStatus.Running, entries.Count, count);

                if (!Entry.IsOverride ||
                    !x.FullName.StartsWith(Entry.Overrides, StringComparison.OrdinalIgnoreCase)) return;

                var subPath = x.FullName[(Entry.Overrides.Length + 1)..];
                if (string.IsNullOrEmpty(subPath))
                    return;

                var filePath = new FileInfo(Path.Combine(Path.GetFullPath(Minecraft.ToWorkingPath(true)), subPath));
                if (x.FullName.EndsWith('/')) {
                    Directory.CreateDirectory(filePath.FullName);
                    return;
                }

                x.ExtractTo(filePath.FullName);
            }
        }, cancellationToken));

        await Task.WhenAll(tasks);
        zipArchive.Dispose();
    }

    #endregion
}