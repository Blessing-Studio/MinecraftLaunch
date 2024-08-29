using MinecraftLaunch.Classes.Models.Download;
using System.Text.Json.Serialization;

namespace MinecraftLaunch.Classes.Models.Game;

public sealed record LibraryJsonEntry {
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("downloads")]
    public DownloadsEntry Downloads { get; set; }

    [JsonPropertyName("natives")]
    public Dictionary<string, string> Natives { get; set; }
}

public sealed record RuleModel {
    [JsonPropertyName("action")]
    public string Action { get; set; }

    [JsonPropertyName("os")]
    public Dictionary<string, string> System { get; set; }
}

[JsonSerializable(typeof(LibraryJsonEntry))]
internal sealed partial class LibraryJsonEntryContext : JsonSerializerContext;