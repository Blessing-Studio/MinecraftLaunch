using System.Text.Json.Serialization;

namespace MinecraftLaunch.Classes.Models.Install {
    public record ForgeInstallEntry {
        [JsonPropertyName("build")]
        public int Build { get; set; }

        [JsonPropertyName("branch")]
        public string Branch { get; set; }

        [JsonPropertyName("mcversion")]
        public string McVersion { get; set; }

        [JsonPropertyName("version")]
        public string ForgeVersion { get; set; }

        [JsonPropertyName("modified")]
        public DateTime ModifiedTime { get; set; }
    }
    
    [JsonSerializable(typeof(ForgeInstallEntry))]
    partial class ForgeInstallEntryContext : JsonSerializerContext;
}
