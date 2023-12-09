using System.Text.Json;
using System.Text.Json.Serialization;

namespace MinecraftLaunch.Classes.Models.Launch {
    public record ArgumentsJsonEntry {
        [JsonPropertyName("jvm")]
        public List<JsonElement> Jvm { get; set; }

        [JsonPropertyName("game")]
        public List<JsonElement> Game { get; set; }
    }
}
