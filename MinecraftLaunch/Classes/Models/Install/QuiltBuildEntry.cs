using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MinecraftLaunch.Classes.Models.Install {
    public record QuiltBuildEntry {
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

    public record QuiltLauncherMeta {
        [JsonPropertyName("mainClass")]
        public Dictionary<string, string> MainClass { get; set; }
    }

    public record QuiltMavenItem {
        [JsonPropertyName("separator")]
        public string Separator { get; set; }

        [JsonPropertyName("maven")]
        public string Maven { get; set; }

        [JsonPropertyName("version")]
        public string Version { get; set; }
    }
    
    [JsonSerializable(typeof(QuiltBuildEntry))]
    partial class QuiltBuildEntryContext : JsonSerializerContext;
}
