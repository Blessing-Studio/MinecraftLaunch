using MinecraftLaunch.Classes.Enums;
using System.Text.Json.Serialization;

namespace MinecraftLaunch.Classes.Models.Download {
    public record CurseForgeResourceEntry {
        [JsonIgnore]
        public string WebLink { get; set; }

        [JsonIgnore]
        public string IconUrl { get; set; }

        [JsonIgnore]
        public IEnumerable<string> Authors { get; set; }

        [JsonIgnore]
        public IEnumerable<string> Categories { get; set; }

        [JsonIgnore]
        public IEnumerable<string> ScreenshotUrls { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("classId")]
        public int ClassId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("summary")]
        public string Summary { get; set; }

        [JsonPropertyName("downloadCount")]
        public int DownloadCount { get; set; }

        [JsonPropertyName("dateModified")]
        public DateTime DateModified { get; set; }

        [JsonPropertyName("latestFilesIndexes")]
        public IEnumerable<CurseFileEntry> Files { get; set; }
    }

    public record CurseFileEntry {
        [JsonPropertyName("fileId")]
        public int FileId { get; set; }

        [JsonPropertyName("gameVersion")]
        public string McVersion { get; set; }

        [JsonPropertyName("filename")]
        public string FileName { get; set; }

        [JsonPropertyName("modLoader")]
        public LoaderType ModLoaderType { get; set; }

        public int ModId { get; set; }

        public string DisplayDescription => $"{ModLoaderType} {McVersion}";
    }
    
    [JsonSerializable(typeof(CurseForgeResourceEntry))]
    partial class CurseForgeResourceEntryContext : JsonSerializerContext;
}
