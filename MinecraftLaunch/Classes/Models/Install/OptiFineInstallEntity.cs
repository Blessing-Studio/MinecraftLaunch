using System.Text.Json.Serialization;

namespace MinecraftLaunch.Classes.Models.Install;

public sealed record OptiFineInstallEntity {
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("patch")]
    public string Patch { get; set; }

    [JsonPropertyName("filename")]
    public string FileName { get; set; }

    [JsonPropertyName("mcversion")]
    public string McVersion { get; set; }
}