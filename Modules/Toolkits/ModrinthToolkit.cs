using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MinecraftLaunch.Modules.Models.Download;
using Natsurainko.Toolkits.Network;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace MinecraftLaunch.Modules.Toolkits;

public class ModrinthToolkit
{
	public static async ValueTask<ModrinthSearchResponse> GetFeaturedModpacksAsync()
	{
		var res = await HttpWrapper.HttpGetAsync("https://api.modrinth.com/v2/search");
        return JsonSerializer.Deserialize<ModrinthSearchResponse>(await res.Content.ReadAsStringAsync())!;
	}

	public static async ValueTask<ModrinthSearchResponse> SearchAsync(string searchFilter, string Category = "", string Index = "relevance", string ProjectType = "mod")
	{
		StringBuilder sb = new StringBuilder($"?query={searchFilter ?? "any"}&index={Index}&facets=[");
		string projType = "[\"project_type:" + ProjectType + "\"]";
		if (!string.IsNullOrEmpty(Category))
		{
			StringBuilder stringBuilder = sb;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(16, 1, stringBuilder);
			handler.AppendLiteral("[\"categories:");
			handler.AppendFormatted(Category);
			handler.AppendLiteral("\"],");
			stringBuilder.Append(ref handler);
		}
		sb.Append(projType);
		sb.Append(']');
		return JsonSerializer.Deserialize<ModrinthSearchResponse>(await (await HttpWrapper.HttpGetAsync($"{"https://api.modrinth.com/v2"}/search{sb}", (Tuple<string, string>)null, HttpCompletionOption.ResponseContentRead)).Content.ReadAsStringAsync());
	}

	public static async ValueTask<ModrinthSearchResponse> SearchModpacksAsync(string searchFilter, string Category = "", string Index = "relevance", int? Offset = null, int? Limit = null)
	{
		StringBuilder sb = new StringBuilder($"?query={searchFilter ?? "any"}&index={Index}&facets=[");
		string projType = "[\"project_type:mod\"]";
		if (!string.IsNullOrEmpty(Category))
		{
			StringBuilder stringBuilder = sb;
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(16, 1, stringBuilder);
			handler.AppendLiteral("[\"categories:");
			handler.AppendFormatted(Category);
			handler.AppendLiteral("\"],");
			stringBuilder2.Append(ref handler);
		}
		sb.Append(projType);
		sb.Append(']');
		string obj = $"{"https://api.modrinth.com/v2"}/search{sb}";
		if (Offset.HasValue)
		{
			StringBuilder stringBuilder = sb;
			StringBuilder stringBuilder3 = stringBuilder;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(8, 1, stringBuilder);
			handler.AppendLiteral("&offset=");
			handler.AppendFormatted(Offset);
			stringBuilder3.Append(ref handler);
		}
		if (Limit.HasValue)
		{
			StringBuilder stringBuilder = sb;
			StringBuilder stringBuilder4 = stringBuilder;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(7, 1, stringBuilder);
			handler.AppendLiteral("&limit=");
			handler.AppendFormatted(Limit);
			stringBuilder4.Append(ref handler);
		}
		return JsonSerializer.Deserialize<ModrinthSearchResponse>(await (await HttpWrapper.HttpGetAsync(obj, (Tuple<string, string>)null, HttpCompletionOption.ResponseContentRead)).Content.ReadAsStringAsync());
	}

	public static async ValueTask<List<string>> GetCategories()
	{
		List<ModrinthCategoryInfo> resModel = JsonSerializer.Deserialize<List<ModrinthCategoryInfo>>(await (await HttpWrapper.HttpGetAsync("https://api.modrinth.com/v2/tag/category", (Tuple<string, string>)null, HttpCompletionOption.ResponseContentRead)).Content.ReadAsStringAsync());
		return (resModel == null) ? new List<string>() : resModel.Select((ModrinthCategoryInfo c) => c.Name).ToList();
	}

	public static async ValueTask<ModrinthProjectInfo> GetProject(string projectId)
	{
		return JsonSerializer.Deserialize<ModrinthProjectInfo>(await (await HttpWrapper.HttpGetAsync("https://api.modrinth.com/v2/project/" + projectId, (Tuple<string, string>)null, HttpCompletionOption.ResponseContentRead)).Content.ReadAsStringAsync());
	}

	public static async ValueTask<List<ModrinthProjectInfoItem>> GetProjectInfos(string projectId)
	{
		return JsonSerializer.Deserialize<List<ModrinthProjectInfoItem>>(await (await HttpWrapper.HttpGetAsync("https://api.modrinth.com/v2/project/" + projectId + "/version", (Tuple<string, string>)null, HttpCompletionOption.ResponseContentRead)).Content.ReadAsStringAsync());
	}
}
