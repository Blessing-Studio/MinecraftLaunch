using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using MinecraftLaunch.Classes.Models.Launch;

namespace MinecraftLaunch.Classes.Models.Game {
    public record GameJsonEntry {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("mainClass")]
        public string MainClass { get; set; }

        [JsonPropertyName("minecraftArguments")]
        public string MinecraftArguments { get; set; }

        [JsonPropertyName("inheritsFrom")]
        public string InheritsFrom { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("assets")]
        public string Assets { get; set; }

        [JsonPropertyName("javaVersion")]
        public JsonNode JavaVersion {  get; set; }

        [JsonPropertyName("arguments")]
        public ArgumentsJsonEntry Arguments { get; set; }

        [JsonPropertyName("assetIndex")]
        public AssstIndex AssetIndex { get; set; }

        [JsonPropertyName("libraries")]
        public JsonArray Libraries { get; set; }
    }

    public record AssstIndex {
        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("sha1")]
        public string Sha1 { get; set; }
    }
}
