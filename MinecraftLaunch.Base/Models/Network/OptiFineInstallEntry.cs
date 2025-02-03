using System.Text.Json.Serialization;

namespace MinecraftLaunch.Base.Models.Network;

public record OptifineInstallEntry {
    [JsonPropertyName("type")] public string Type { get; set; }
    [JsonPropertyName("patch")] public string Patch { get; set; }
    [JsonPropertyName("filename")] public string FileName { get; set; }
    [JsonPropertyName("mcversion")] public string McVersion { get; set; }
}

[JsonSerializable(typeof(OptifineInstallEntry))]
[JsonSerializable(typeof(IEnumerable<OptifineInstallEntry>))]
public sealed partial class OptifineInstallEntryContext : JsonSerializerContext;