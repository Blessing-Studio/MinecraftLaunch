using Flurl.Http;
using MinecraftLaunch.Classes.Enums;
using MinecraftLaunch.Classes.Interfaces;
using MinecraftLaunch.Classes.Models.Download;
using MinecraftLaunch.Extensions;
using MinecraftLaunch.Utilities;
using System.Text;
using System.Text.Json;

namespace MinecraftLaunch.Components.Fetcher;

/// <summary>
/// Fetches resources from Modrinth.
/// </summary>
public sealed class ModrinthFetcher : IFetcher<IEnumerable<ModrinthResourceEntry>> {
    private const string BASE_API = "https://api.modrinth.com/v2/";

    /// <summary>
    /// Fetches the Modrinth resources synchronously.
    /// </summary>
    /// <returns>An enumerable collection of Modrinth resources.</returns>
    public IEnumerable<ModrinthResourceEntry> Fetch() {
        return FetchAsync().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Fetches the Modrinth resources asynchronously.
    /// </summary>
    /// <returns>A ValueTask that represents the asynchronous operation. The task result contains an enumerable collection of Modrinth resources.</returns>
    public async ValueTask<IEnumerable<ModrinthResourceEntry>> FetchAsync() {
        var jNode = (await $"{BASE_API}search".GetStringAsync()).AsNode();
        return jNode?.Select("hits")?.GetEnumerable()
            .Deserialize<IEnumerable<ModrinthResourceEntry>>(JsonConverterUtil.DefaultJsonOptions);
    }

    /// <summary>
    /// Searches for Modrinth resources asynchronously based on the provided search filter and other parameters.
    /// </summary>
    /// <param name="searchFilter">The search filter.</param>
    /// <param name="version">The version. Defaults to null.</param>
    /// <param name="resourceType">The resource type. Defaults to ModrinthResourceType.Mod.</param>
    /// <returns>A ValueTask that represents the asynchronous operation. The task result contains an enumerable collection of Modrinth resources.</returns>
    public async ValueTask<IEnumerable<ModrinthResourceEntry>> SearchResourcesAsync(
        string searchFilter,
        string version = default,
        ModrinthResourceType? resourceType = ModrinthResourceType.Mod) {
        var stringBuilder = new StringBuilder(BASE_API);
        stringBuilder.Append($"search?query={searchFilter}");

        var facets = new List<string>();

        if (resourceType != null) {
            facets.Add($"[\"project_type:{resourceType switch {
                ModrinthResourceType.ModPack => "modpack",
                ModrinthResourceType.Resourcepack => "resourcepack",
                _ => "mod"
            }}\"]");
        }

        if (version != null) {
            facets.Add($"\"[versions:{version}\"]");
        }

        if (facets.Any()) {
            stringBuilder.Append($"&facets=[{string.Join(',', facets)}]");
        }

        var jNode = (await stringBuilder.ToString().GetStringAsync()).AsNode();
        return jNode?.Select("hits")?.GetEnumerable()
            .Deserialize<IEnumerable<ModrinthResourceEntry>>(JsonConverterUtil.DefaultJsonOptions);
    }
}