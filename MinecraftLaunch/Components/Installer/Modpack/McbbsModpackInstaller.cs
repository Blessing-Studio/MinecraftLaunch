﻿using MinecraftLaunch.Base.Enums;
using MinecraftLaunch.Base.Interfaces;
using MinecraftLaunch.Base.Models.Game;
using MinecraftLaunch.Base.Models.Network;
using MinecraftLaunch.Extensions;
using System.IO.Compression;
using System.Runtime.CompilerServices;

namespace MinecraftLaunch.Components.Installer.Modpack;

public sealed class McbbsModpackInstaller : InstallerBase {
    public string ModpackPath { get; init; }
    public MinecraftEntry Minecraft { get; init; }
    public McbbsModpackInstallEntry Entry { get; init; }
    public override string MinecraftFolder { get; init; }

    public static McbbsModpackInstaller Create(string mcFolder, string modpackPath, McbbsModpackInstallEntry installEntry, MinecraftEntry entry) {
        return new McbbsModpackInstaller {
            MinecraftFolder = mcFolder,
            ModpackPath = modpackPath,
            Entry = installEntry,
            Minecraft = entry
        };
    }

    public static async IAsyncEnumerable<IInstallEntry> ParseModLoaderEntryAsync(McbbsModpackInstallEntry modpack, [EnumeratorCancellation] CancellationToken cancellationToken = default) {
        foreach (var addon in modpack.Addons) {
            cancellationToken.ThrowIfCancellationRequested();

            switch (addon.Id) {
                case "fabric":
                    yield return await FabricInstaller.EnumerableFabricAsync(modpack.McVersion, cancellationToken)
                        .FirstOrDefaultAsync(x => x.BuildVersion.Equals(addon.Version), cancellationToken);
                    break;
                case "quilt":
                    yield return await QuiltInstaller.EnumerableQuiltAsync(modpack.McVersion, cancellationToken)
                        .FirstOrDefaultAsync(x => x.BuildVersion.Equals(addon.Version), cancellationToken);
                    break;
                case "forge":
                    yield return await ForgeInstaller.EnumerableForgeAsync(modpack.McVersion, false, cancellationToken)
                        .FirstOrDefaultAsync(x => x.ForgeVersion.Equals(addon.Version), cancellationToken);
                    break;
                case "neoforge":
                    yield return await ForgeInstaller.EnumerableForgeAsync(modpack.McVersion, true, cancellationToken)
                        .FirstOrDefaultAsync(x => x.ForgeVersion.Equals(addon.Version), cancellationToken);
                    break;
                case "optifine":
                    yield return await OptifineInstaller.EnumerableOptifineAsync(modpack.McVersion, cancellationToken)
                        .FirstOrDefaultAsync(x => addon.Version.Contains(x.Type), cancellationToken);
                    break;
            }
        }
    }

    public static McbbsModpackInstallEntry ParseModpackInstallEntry(string modpackPath) {
        using var zipArchive = ZipFile.OpenRead(modpackPath);
        var json = zipArchive?.GetEntry("mcbbs.packmeta")?.ReadAsString()
            ?? throw new ArgumentException("Not found mcbbs.packmeta");

        return json.Deserialize(McbbsModpackInstallEntryContext.Default.McbbsModpackInstallEntry)
            ?? throw new InvalidOperationException("Failed to parsemcbbs.packmeta");
    }


    public override async Task<MinecraftEntry> InstallAsync(CancellationToken cancellationToken = default) {
        ReportProgress(InstallStep.Started, 0.0d, TaskStatus.WaitingToRun, 1, 1);

        try {
            await ExtractModpackAsync(cancellationToken);

            ReportProgress(InstallStep.RanToCompletion, 1.0d, TaskStatus.RanToCompletion, 1, 1);
            ReportCompleted();
        } catch (Exception) {
            ReportProgress(InstallStep.Interrupted, 1.0d, TaskStatus.Canceled, 1, 1);
            ReportCompleted();
        }

        return Minecraft;
    }

    #region Privates

    private async Task ExtractModpackAsync(CancellationToken cancellationToken) {
        var zipArchive = ZipFile.OpenRead(ModpackPath);
        var entries = zipArchive?.Entries;
        ReportProgress(InstallStep.ExtractModpack, 0.10d, TaskStatus.Running, entries.Count, 0);

        const string decompressPrefix = "overrides";
        string woringPath = Minecraft.ToWorkingPath(true);

        int count = 0;
        var tasks = entries.Select(x => Task.Run(() => {
            lock (zipArchive) {
                ReportProgress(InstallStep.ExtractModpack,
                    ((double)Interlocked.Increment(ref count) / (double)entries.Count).ToPercentage(0.1d, 1.0d),
                    TaskStatus.Running, entries.Count, count);

                if (!x.FullName.StartsWith(decompressPrefix))
                    return;

                var subPath = x.FullName[(decompressPrefix.Length + 1)..];
                if (string.IsNullOrEmpty(subPath))
                    return;

                var filePath = new FileInfo(Path.Combine(woringPath, subPath));
                if (x.FullName.EndsWith('/')) {
                    filePath.Directory.Create();
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