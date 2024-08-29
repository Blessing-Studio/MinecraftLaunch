using Flurl.Http;
using MinecraftLaunch.Classes.Enums;
using MinecraftLaunch.Classes.Interfaces;
using MinecraftLaunch.Classes.Models.Download;
using MinecraftLaunch.Extensions;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Web;

namespace MinecraftLaunch.Components.Fetcher;

/// <summary>
/// Fetches resources from CurseForge.
/// </summary>
public sealed class CurseForgeFetcher : IFetcher<IEnumerable<CurseForgeResourceEntry>> {
    private const string BASE_API = "https://api.curseforge.com/v1/mods";

    private readonly string _key;

    /// <summary>
    /// Initializes a new instance of the <see cref="CurseForgeFetcher"/> class.
    /// </summary>
    /// <param name="apiKey">The API key for CurseForge.</param>
    public CurseForgeFetcher(string apiKey) {
        _key = apiKey;
    }

    /// <summary>
    /// Fetches the CurseForge resources synchronously.
    /// </summary>
    /// <returns>An enumerable collection of CurseForge resources.</returns>
    public IEnumerable<CurseForgeResourceEntry> Fetch() {
        return FetchAsync().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Fetches the CurseForge resources asynchronously.
    /// </summary>
    /// <returns>A ValueTask that represents the asynchronous operation. The task result contains an enumerable collection of CurseForge resources.</returns>
    public async ValueTask<IEnumerable<CurseForgeResourceEntry>> FetchAsync() {
        var result = new List<CurseForgeResourceEntry>();
        var payload = new {
            gameId = 432,
            excludedModIds = new[] { 0 },
            gameVersionTypeId = null as string
        };
        try {
            using var responseMessage = await $"{BASE_API}/featured"
                .WithHeader("x-api-key", _key)
                .PostJsonAsync(payload);

            var jsonNode = (await responseMessage.GetStringAsync())
                .AsNode().Select("data");

            var resources = jsonNode.GetEnumerable("featured")
                .Union(jsonNode.GetEnumerable("popular"));

            foreach (var resource in resources) {
                result.Add(ResolveFromJsonNode(resource));
            }
        } catch (Exception) { }

        return result;
    }

    /// <summary>
    /// Searches for CurseForge resources asynchronously based on the provided search filter and other parameters.
    /// </summary>
    /// <param name="searchFilter">The search filter.</param>
    /// <param name="classId">The class ID. Defaults to 6.</param>
    /// <param name="category">The category. Defaults to -1.</param>
    /// <param name="gameVersion">The game version. Defaults to null.</param>
    /// <param name="modLoaderType">The mod loader type. Defaults to LoaderType.Any.</param>
    /// <returns>A ValueTask that represents the asynchronous operation. The task result contains an enumerable collection of CurseForge resources.</returns>
    public async ValueTask<IEnumerable<CurseForgeResourceEntry>> SearchResourcesAsync(
        string searchFilter,
        int classId = 6,
        int category = -1,
        string gameVersion = null,
        LoaderType modLoaderType = LoaderType.Any) {
        var stringBuilder = new StringBuilder(BASE_API);
        stringBuilder.Append("/search?gameId=432");
        stringBuilder.Append("&sortField=Featured");
        stringBuilder.Append("&sortOrder=desc");
        stringBuilder.Append($"&categoryId={category}&classId={classId}");
        stringBuilder.Append($"modLoaderType={(int)modLoaderType}");
        stringBuilder.Append($"gameVersion={gameVersion}");
        stringBuilder.Append($"&searchFilter={HttpUtility.UrlEncode(searchFilter)}");

        var jsonNode = (await stringBuilder.ToString()
            .WithHeader("x-api-key", _key)
            .GetStringAsync())
            .AsNode();

        return jsonNode.GetEnumerable("data").Select(ResolveFromJsonNode);
    }

    private CurseForgeResourceEntry ResolveFromJsonNode(JsonNode node) {
        var entry = node.Deserialize<CurseForgeResourceEntry>();

        entry.IconUrl = node.Select("logo")?.GetString("url");
        entry.WebLink = node.Select("links")?.GetString("websiteUrl");
        entry.Authors = node?.GetEnumerable<string>("authors", "name");
        entry.Categories = node?.GetEnumerable<string>("categories", "name");
        entry.ScreenshotUrls = node?.GetEnumerable<string>("screenshots", "url");
        entry.Files = entry.Files.Select(x => {
            x.ModId = entry.Id;
            return x;
        });

        return entry;
    }
}