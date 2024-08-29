using MinecraftLaunch.Classes.Enums;
using MinecraftLaunch.Classes.Models.Game;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace MinecraftLaunch.Classes.Models.Install;

public sealed record FabricBuildEntry {
    [JsonPropertyName("intermediary")]
    public FabricMavenItem Intermediary { get; set; }

    [JsonPropertyName("loader")]
    public FabricMavenItem Loader { get; set; }

    [JsonPropertyName("launcherMeta")]
    public FabricLauncherMeta LauncherMeta { get; set; }

    [JsonIgnore]
    public string McVersion => Intermediary.Version;

    [JsonIgnore]
    public string DisplayVersion => $"{McVersion}-{Loader.Version}";

    [JsonIgnore]
    public string BuildVersion => Loader.Version;

    [JsonIgnore]
    public LoaderType ModLoaderType => LoaderType.Fabric;
}

public sealed record FabricLauncherMeta {
    [JsonPropertyName("mainClass")]
    public JsonNode MainClass { get; set; }

    [JsonPropertyName("libraries")]
    public Dictionary<string, List<LibraryJsonEntry>> Libraries { get; set; }
}

public sealed record FabricMavenItem {
    [JsonPropertyName("separator")]
    public string Separator { get; set; }

    [JsonPropertyName("maven")]
    public string Maven { get; set; }

    [JsonPropertyName("version")]
    public string Version { get; set; }
}

[JsonSerializable(typeof(FabricBuildEntry))]
internal sealed partial class FabricBuildEntryContext : JsonSerializerContext;