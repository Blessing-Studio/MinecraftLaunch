using System.Text.Json.Serialization;

namespace MinecraftLaunch.Classes.Models.Game;

public sealed record AssetJsonEntry {
    [JsonPropertyName("size")]
    public int Size { get; set; }

    [JsonPropertyName("hash")]
    public string Hash { get; set; }
}
    
[JsonSerializable(typeof(AssetJsonEntry))]
sealed partial class AssetJsonEntryContext : JsonSerializerContext;