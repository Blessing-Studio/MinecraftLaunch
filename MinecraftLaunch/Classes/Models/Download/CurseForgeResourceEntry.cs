using MinecraftLaunch.Classes.Enums;
using System.Text.Json.Serialization;

namespace MinecraftLaunch.Classes.Models.Download {
    public record CurseForgeResourceEntry {
        public string WebLink { get; set; }

        public string IconUrl { get; set; }

        public IEnumerable<string> Authors { get; set; }

        public IEnumerable<string> Categories { get; set; }

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
}
