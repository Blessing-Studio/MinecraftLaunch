using System.Text.Json.Serialization;
using MinecraftLaunch.Classes.Models.Download;

namespace MinecraftLaunch.Classes.Models.Game {
    public record LibraryJsonEntry {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("downloads")]
        public DownloadsEntry Downloads { get; set; }

        [JsonPropertyName("natives")]
        public Dictionary<string, string> Natives { get; set; }
    }

    public record RuleModel {
        [JsonPropertyName("action")]
        public string Action { get; set; }

        [JsonPropertyName("os")]
        public Dictionary<string, string> System { get; set; }
    }
}
