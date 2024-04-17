using System.Text.Json.Serialization;

namespace MinecraftLaunch.Classes.Models.ServerPing;

public sealed class Player {
    [JsonPropertyName("id")] public string Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; }
}