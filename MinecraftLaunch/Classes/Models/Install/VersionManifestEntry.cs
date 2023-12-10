using System.Text.Json.Serialization;

namespace MinecraftLaunch.Classes.Models.Install {
    public record VersionManifestEntry {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("time")]
        public DateTime Time { get; set; }

        [JsonPropertyName("releaseTime")]
        public DateTime ReleaseTime { get; set; }
    }
}
