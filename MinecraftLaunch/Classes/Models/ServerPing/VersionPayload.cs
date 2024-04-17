using System.Text.Json.Serialization;

namespace MinecraftLaunch.Classes.Models.ServerPing;

public sealed class VersionPayload {
    [JsonPropertyName("name")] public string Name { get; set; }
    [JsonPropertyName("protocol")] public int Protocol { get; set; }
}