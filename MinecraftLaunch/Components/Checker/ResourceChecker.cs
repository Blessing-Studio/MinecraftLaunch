using MinecraftLaunch.Classes.Interfaces;
using MinecraftLaunch.Classes.Models.Game;
using MinecraftLaunch.Components.Resolver;
using MinecraftLaunch.Extensions;
using System.Collections.Immutable;

namespace MinecraftLaunch.Components.Checker;

/// <summary>
/// A checker for Minecraft game resources.
/// </summary>
/// <remarks>
/// This includes both Assets and Libraries.
/// </remarks>
public sealed class ResourceChecker(GameEntry entry) : IChecker {
    private AssetsResolver AssetsResolver => new(entry);
    private LibrariesResolver LibraryResolver => new(entry);

    /// <summary>
    /// Gets the collection of missing resources.
    /// </summary>
    public IReadOnlyCollection<IDownloadEntry> MissingResources { get; private set; }

    /// <summary>
    /// Checks the game resources asynchronously.
    /// </summary>
    /// <returns>
    /// A ValueTask that represents the asynchronous operation. The task result contains a boolean value indicating whether all resources are present.
    /// </returns>
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

        return MissingResources.Count == 0;
    }
}