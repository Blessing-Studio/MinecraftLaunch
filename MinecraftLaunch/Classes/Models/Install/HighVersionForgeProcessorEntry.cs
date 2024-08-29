using System.Text.Json.Serialization;

namespace MinecraftLaunch.Classes.Models.Install;

public sealed record HighVersionForgeProcessorEntry {
    [JsonPropertyName("jar")]
    public string Jar { get; set; }

    [JsonPropertyName("sides")]
    public List<string> Sides { get; set; } = new();

    [JsonPropertyName("classpath")]
    public IEnumerable<string> Classpath { get; set; }

    [JsonPropertyName("args")]
    public IEnumerable<string> Args { get; set; }

    [JsonPropertyName("outputs")]
    public Dictionary<string, string> Outputs { get; set; } = new();
}

[JsonSerializable(typeof(HighVersionForgeProcessorEntry))]
internal sealed partial class HighVersionForgeProcessorEntryContext : JsonSerializerContext;