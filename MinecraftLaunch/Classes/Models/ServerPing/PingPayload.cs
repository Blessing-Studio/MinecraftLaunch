using System.Text.Json;
using System.Text.Json.Serialization;

namespace MinecraftLaunch.Classes.Models.ServerPing;

public sealed class PingPayload {
    [JsonPropertyName("favicon")] public string Icon { get; set; }
    [JsonPropertyName("version")] public VersionPayload Version { get; set; }
    [JsonPropertyName("players")] public PlayersPayload Players { get; set; }
    [JsonPropertyName("modinfo")] public ServerPingModInfo ModInfo { get; set; }
    [JsonPropertyName("description")] public JsonElement Description { get; set; }
}

[JsonSerializable(typeof(PingPayload))]
sealed partial class PingPayloadContext : JsonSerializerContext;