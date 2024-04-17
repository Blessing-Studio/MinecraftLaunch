using System.Text.Json.Serialization;

namespace MinecraftLaunch.Classes.Models.ServerPing;

public sealed class ServerPingModInfo {
    [JsonPropertyName("type")] public string Type { get; set; }
    [JsonPropertyName("modList")] public IEnumerable<ModInfo> ModList { get; set; }
}

public sealed class ModInfo {
    [JsonPropertyName("modid")] public string ModId { get; set; }
    [JsonPropertyName("version")] public string Version { get; set; }
}