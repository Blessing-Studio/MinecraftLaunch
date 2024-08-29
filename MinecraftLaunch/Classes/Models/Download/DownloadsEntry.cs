using System.Text.Json.Serialization;

namespace MinecraftLaunch.Classes.Models.Download;

public sealed record DownloadsEntry {
    [JsonPropertyName("artifact")]
    public FileEntry Artifact { get; set; }

    [JsonPropertyName("classifiers")]
    public Dictionary<string, FileEntry> Classifiers { get; set; }
}

public sealed record FileEntry {
    [JsonPropertyName("path")]
    public string Path { get; set; }

    [JsonPropertyName("sha1")]
    public string Sha1 { get; set; }

    [JsonPropertyName("size")]
    public int Size { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }

    //for client-x.xx.xml
    [JsonPropertyName("id")]
    public string Id { get; set; }
}

[JsonSerializable(typeof(DownloadsEntry))]
internal sealed partial class DownloadsEntryContext : JsonSerializerContext;