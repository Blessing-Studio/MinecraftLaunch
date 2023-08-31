using MinecraftLaunch.Modules.Models.Launch;
using System.Text.Json.Serialization;

namespace MinecraftLaunch.Modules.Models.Download;

public class DownloadsJsonEntity
{
	[JsonPropertyName("artifact")]
	public FileJsonEntity Artifact { get; set; }

	[JsonPropertyName("classifiers")]
	public Dictionary<string, FileJsonEntity> Classifiers { get; set; }
}
