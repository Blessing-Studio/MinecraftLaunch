using System.Text.Json.Serialization;
using MinecraftLaunch.Classes.Enums;
using MinecraftLaunch.Classes.Models.Download;

namespace MinecraftLaunch.Classes.Models.Game;

public sealed record LibraryJsonEntry {
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("downloads")]
    public DownloadsEntry Downloads { get; set; }

    [JsonPropertyName("natives")]
    public Dictionary<Platform, string> Natives { get; set; }
}

public sealed record RuleModel
{
    [JsonPropertyName("action")]
    public string Action { get; set; }

    [JsonPropertyName("os")]
    public Dictionary<string, Platform> System { get; set; }
}
    
[JsonSerializable(typeof(LibraryJsonEntry))]
sealed partial class LibraryJsonEntryContext : JsonSerializerContext;