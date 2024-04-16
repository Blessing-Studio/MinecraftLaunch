using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Web;
using Flurl.Http;
using MinecraftLaunch.Classes.Enums;
using MinecraftLaunch.Extensions;
using MinecraftLaunch.Classes.Interfaces;
using MinecraftLaunch.Classes.Models.Download;

namespace MinecraftLaunch.Components.Fetcher;

public sealed class CurseForgeFetcher(string apiKey) : IFetcher<IEnumerable<CurseForgeResourceEntry>> {
    private readonly string _key = apiKey;
    private readonly string _api = "https://api.curseforge.com/v1/mods";
    
    public IEnumerable<CurseForgeResourceEntry> Fetch() {
        return FetchAsync().GetAwaiter().GetResult();
    }

    public async ValueTask<IEnumerable<CurseForgeResourceEntry>> FetchAsync() {
        var result = new List<CurseForgeResourceEntry>();
        var payload = new {
            gameId = 432,
            excludedModIds = new[] { 0 },
            gameVersionTypeId = null as string
        };
        try {
            using var responseMessage = await $"{_api}/featured"
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

    public async ValueTask<IEnumerable<CurseForgeResourceEntry>> SearchResourcesAsync(
        string searchFilter,
        int classId = 6, 
        int category = -1,
        string gameVersion = null, 
        LoaderType modLoaderType = LoaderType.Any) {
        var stringBuilder = new StringBuilder(_api);
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