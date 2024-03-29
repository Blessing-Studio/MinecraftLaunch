﻿using Flurl.Http;
using System.Text;
using System.Text.Json;
using MinecraftLaunch.Utilities;
using MinecraftLaunch.Extensions;
using MinecraftLaunch.Classes.Enums;
using MinecraftLaunch.Classes.Interfaces;
using MinecraftLaunch.Classes.Models.Download;

namespace MinecraftLaunch.Components.Fetcher; 
public sealed class ModrinthFetcher : IFetcher<IEnumerable<ModrinthResourceEntry>> {
    private readonly string _api = "https://api.modrinth.com/v2/";
    
    public IEnumerable<ModrinthResourceEntry> Fetch() {
        return FetchAsync().GetAwaiter().GetResult();
    }

    public async ValueTask<IEnumerable<ModrinthResourceEntry>> FetchAsync() {
        var jNode = (await $"{_api}search".GetStringAsync()).AsNode();
        return jNode?.Select("hits")?.GetEnumerable()
            .Deserialize<IEnumerable<ModrinthResourceEntry>>(JsonConverterUtil.DefaultJsonOptions);
    }
    
    public async ValueTask<IEnumerable<ModrinthResourceEntry>> SearchResourcesAsync(
        string searchFilter,
        string version = default,
        ModrinthResourceType? resourceType = ModrinthResourceType.Mod) {
        var stringBuilder = new StringBuilder(_api);
        stringBuilder.Append($"search?query={searchFilter}");

        var facets = new List<string>();

        if (resourceType != null) {
            facets.Add($"[\"project_type:{resourceType switch
            {
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