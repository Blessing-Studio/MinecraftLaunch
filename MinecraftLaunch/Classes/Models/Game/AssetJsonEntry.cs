using System.Text.Json.Serialization;

namespace MinecraftLaunch.Classes.Models.Game {
    public record AssetJsonEntry {
        [JsonPropertyName("size")]
        public int Size { get; set; }

        [JsonPropertyName("hash")]
        public string Hash { get; set; }
    }
    
    [JsonSerializable(typeof(AssetJsonEntry))]
    partial class AssetJsonEntryContext : JsonSerializerContext;
}
