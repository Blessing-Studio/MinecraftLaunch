using System.Text.Json.Serialization;

namespace MinecraftLaunch.Classes.Models.Install;

public sealed record QuiltBuildEntry {
    [JsonPropertyName("intermediary")]
    public QuiltMavenItem Intermediary { get; set; }

    [JsonPropertyName("loader")]
    public QuiltMavenItem Loader { get; set; }

    [JsonPropertyName("launcherMeta")]
    public QuiltLauncherMeta LauncherMeta { get; set; }

    [JsonIgnore]
    public string BuildVersion => Loader.Version;

    [JsonIgnore]
    public string McVersion => Intermediary.Version;

    [JsonIgnore]
    public string DisplayVersion => $"{McVersion}-{Loader.Version}";
}

public sealed record QuiltLauncherMeta {
    [JsonPropertyName("mainClass")]
    public Dictionary<string, string> MainClass { get; set; }
}

public sealed record QuiltMavenItem {
    [JsonPropertyName("separator")]
    public string Separator { get; set; }

    [JsonPropertyName("maven")]
    public string Maven { get; set; }

    [JsonPropertyName("version")]
    public string Version { get; set; }
}

[JsonSerializable(typeof(QuiltBuildEntry))]
internal sealed partial class QuiltBuildEntryContext : JsonSerializerContext;