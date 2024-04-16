using MinecraftLaunch.Extensions;
using System.Collections.Immutable;
using MinecraftLaunch.Classes.Interfaces;
using MinecraftLaunch.Classes.Models.Game;
using MinecraftLaunch.Components.Resolver;

namespace MinecraftLaunch.Components.Checker;

/// <summary>
/// Minecraft 游戏资源检查器
/// </summary>
/// <remarks>
/// 包含 Assets 和 Libraries
/// </remarks>
public sealed class ResourceChecker(GameEntry entry) : IChecker {
    private AssetsResolver AssetsResolver => new(entry);

    private LibrariesResolver LibraryResolver => new(entry);

    public IReadOnlyCollection<IDownloadEntry> MissingResources { get; private set; }

    public async ValueTask<bool> CheckAsync() {
        var assetIndex = AssetsResolver.GetAssetIndexJson();
        if (!assetIndex.Verify()) {
            await assetIndex!.DownloadResourceEntryAsync();
        }

        var assets = AssetsResolver.GetAssets();
        var libraries = LibraryResolver.GetLibraries();

        IEnumerable<IDownloadEntry> entries = [.. assets, .. libraries];
        MissingResources = entries.AsParallel()
            .Where(entry => !entry.Verify())
            .ToImmutableArray();

        return !MissingResources.Any();
    }
}