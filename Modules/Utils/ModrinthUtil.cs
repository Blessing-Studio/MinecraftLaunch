using MinecraftLaunch.Modules.Models.Download;
using System.Text.Json.Serialization;
using System.Text;
using System.Text.Json;
using Flurl.Http;
using System.Collections.Generic;
using System.Xml.Linq;

namespace MinecraftLaunch.Modules.Utils;

public class ModrinthUtil {
    public static async ValueTask<ModrinthSearchResponse> GetFeaturedModpacksAsync() {
        using var responseMessage = await "https://api.modrinth.com/v2/search".GetAsync();
        return JsonSerializer.Deserialize<ModrinthSearchResponse>(await responseMessage.GetStringAsync())!;
    }

    public static async ValueTask<ModrinthSearchResponse> SearchAsync(string searchFilter, string Category = "", string Index = "relevance", string ProjectType = "mod") {
        var builder = new StringBuilder($"?query={searchFilter ?? "any"}&index={Index}&facets=[");
        var projType = $"[\"project_type:{ProjectType}\"]";

        if (!string.IsNullOrEmpty(Category)) {
            builder.Append($"[\"categories:{Category}\"],");
        }

        builder.Append(projType);
        builder.Append(']');

        using var responseMessage = await $"{"https://api.modrinth.com/v2"}/search{builder}"
            .GetAsync();

        return JsonSerializer.Deserialize<ModrinthSearchResponse>(await responseMessage.GetStringAsync())!;
    }

    public static async ValueTask<ModrinthSearchResponse> SearchModpacksAsync(string searchFilter, string Category = "", string Index = "relevance") {
        var builder = new StringBuilder($"?query={searchFilter ?? "any"}&index={Index}&facets=[");
        var projType = $"[\"project_type:mod\"]";

        if (!string.IsNullOrEmpty(Category)) {
            builder.Append($"[\"categories:{Category}\"],");
        }
        builder.Append(projType);
        builder.Append(']');

        string url = $"{"https://api.modrinth.com/v2"}/search{builder}";
        using var responseMessage = await url.GetAsync();

        return JsonSerializer.Deserialize<ModrinthSearchResponse>(await responseMessage.GetStringAsync())!;
    }

    public static async ValueTask<List<string>> GetCategories() {
        string url = "https://api.modrinth.com/v2/tag/category";
        using var responseMessage = await url.GetAsync();
        var resModel = JsonSerializer.Deserialize<List<ModrinthCategoryInfo>>(await responseMessage.GetStringAsync())!;

        return resModel == null ? new() : resModel.Select((ModrinthCategoryInfo c) => c.Name).ToList();
    }

    public static async ValueTask<ModrinthProjectInfo> GetProject(string projectId) {
        string url = $"https://api.modrinth.com/v2/project/{projectId}";
        using var responseMessage = await url.GetAsync();

        return JsonSerializer.Deserialize<ModrinthProjectInfo>(await responseMessage.GetStringAsync())!;
    }

    public static async ValueTask<List<ModrinthProjectInfoItem>> GetProjectInfos(string projectId) {
        string url = $"https://api.modrinth.com/v2/project/{projectId}/version";
        using var responseMessage = await url.GetAsync();

        return JsonSerializer.Deserialize<List<ModrinthProjectInfoItem>>(await responseMessage.GetStringAsync())!;
    }
}
