using MinecraftLaunch.Modules.Models.Download;
using System.Text.Json.Serialization;
using System.Text;
using System.Text.Json;
using Flurl.Http;
using System.Collections.Generic;
using System.Xml.Linq;

namespace MinecraftLaunch.Modules.Utilities;

public class ModrinthUtil {
    public static async ValueTask<ModrinthSearchResponse> GetFeaturedModpacksAsync() {
        var response = await "https://api.modrinth.com/v2/search".GetAsync();
        return (await response.GetStringAsync())
            .ToJsonEntity<ModrinthSearchResponse>();
    }

    public static async ValueTask<ModrinthSearchResponse> SearchAsync(string searchFilter, string category = "", string index = "relevance", string projectType = "mod") {
        var builder = new StringBuilder($"?query={searchFilter ?? "any"}&index={index}&facets=[");
        var projType = $"[\"project_type:{projectType}\"]";

        if (!string.IsNullOrEmpty(category)) {
            builder.Append($"[\"categories:{category}\"],");
        }

        builder.Append(projType);
        builder.Append(']');

        var response = await $"https://api.modrinth.com/v2/search{builder}"
            .GetAsync();

        return (await response.GetStringAsync())
            .ToJsonEntity<ModrinthSearchResponse>()!;
    }

    public static async ValueTask<ModrinthSearchResponse> SearchModpacksAsync(string searchFilter, string category = "", string index = "relevance") {
        return await SearchAsync(searchFilter, category, index, "mod");
    }

    public static async ValueTask<List<string>> GetCategories() {
        var response = await "https://api.modrinth.com/v2/tag/category".GetAsync();
        var categories = (await response.GetStringAsync()).ToJsonEntity<List<ModrinthCategoryInfo>>();

        return categories?.Select(c => c.Name).ToList() ?? new List<string>();
    }

    public static async ValueTask<ModrinthProjectInfo> GetProject(string projectId) {
        var response = await $"https://api.modrinth.com/v2/project/{projectId}".GetAsync();
        return (await response.GetStringAsync()).ToJsonEntity<ModrinthProjectInfo>();
    }

    public static async ValueTask<List<ModrinthProjectInfoItem>> GetProjectInfos(string projectId) {
        var response = await $"https://api.modrinth.com/v2/project/{projectId}/version".GetAsync();
        return (await response.GetStringAsync()).ToJsonEntity<List<ModrinthProjectInfoItem>>();
    }
}
