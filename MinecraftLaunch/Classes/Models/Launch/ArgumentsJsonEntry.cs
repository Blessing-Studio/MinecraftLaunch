using System.Text.Json;
using System.Text.Json.Serialization;

namespace MinecraftLaunch.Classes.Models.Launch;

public sealed record ArgumentsJsonEntry {
    [JsonPropertyName("jvm")]
    public List<JsonElement> Jvm { get; set; }

    [JsonPropertyName("game")]
    public List<JsonElement> Game { get; set; }
}

[JsonSerializable(typeof(ArgumentsJsonEntry))]
internal sealed partial class ArgumentsJsonEntryContext : JsonSerializerContext;