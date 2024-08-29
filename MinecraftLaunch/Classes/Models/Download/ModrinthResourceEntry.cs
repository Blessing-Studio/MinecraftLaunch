using System.Text.Json.Serialization;

namespace MinecraftLaunch.Classes.Models.Download;

public sealed record ModrinthResourceEntry {
    [JsonPropertyName("project_id")]
    public string Id { get; set; }

    [JsonPropertyName("slug")]
    public string Slug { get; set; }

    [JsonPropertyName("title")]
    public string Name { get; set; }

    [JsonPropertyName("author")]
    public string Author { get; set; }

    [JsonPropertyName("description")]
    public string Summary { get; set; }

    [JsonPropertyName("icon_url")]
    public string IconUrl { get; set; }

    [JsonPropertyName("downloads")]
    public int DownloadCount { get; set; }

    [JsonPropertyName("project_type")]
    public string ProjectType { get; set; }

    [JsonPropertyName("date_modified")]
    public DateTime DateModified { get; set; }

    [JsonPropertyName("display_categories")]
    public IEnumerable<string> Categories { get; set; }

    [JsonPropertyName("gallery")]
    public IEnumerable<string> ScreenshotUrls { get; set; }

    [JsonIgnore]
    public string WebLink => $"https://modrinth.com/{ProjectType}/{Slug}";
}

[JsonSerializable(typeof(ModrinthResourceEntry))]
internal sealed partial class ModrinthResourceEntryContext : JsonSerializerContext;