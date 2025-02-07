using MinecraftLaunch.Base.Enums;
using MinecraftLaunch.Base.Interfaces;
using System.Text.Json.Serialization;

namespace MinecraftLaunch.Base.Models.Network;

public record ForgeInstallEntry : IInstallEntry {
    [JsonIgnore] public bool IsNeoforge { get; set; }
    [JsonPropertyName("build")] public int Build { get; set; }
    [JsonPropertyName("branch")] public string Branch { get; set; }
    [JsonPropertyName("mcversion")] public string McVersion { get; set; }
    [JsonPropertyName("version")] public string ForgeVersion { get; set; }
    [JsonPropertyName("modified")] public DateTime ModifiedTime { get; set; }

    [JsonIgnore] public ModLoaderType ModLoaderType => ModLoaderType.Forge;
}

[JsonSerializable(typeof(ForgeInstallEntry))]
[JsonSerializable(typeof(IEnumerable<ForgeInstallEntry>))]
public sealed partial class ForgeInstallEntryContext : JsonSerializerContext;