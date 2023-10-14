using System.Text.Json.Serialization;

namespace MinecraftLaunch.Modules.Parser;

public class AssetsJsonEntity {
    [JsonPropertyName("hash")]
    public string Hash { get; set; }

    [JsonPropertyName("size")]
    public int Size { get; set; }
}
