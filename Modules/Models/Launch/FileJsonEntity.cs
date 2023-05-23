using System.Text.Json.Serialization;

namespace MinecraftLaunch.Modules.Models.Launch;

public class FileJsonEntity
{
	[JsonPropertyName("path")]
	public string Path { get; set; }

	[JsonPropertyName("sha1")]
	public string Sha1 { get; set; }

	[JsonPropertyName("size")]
	public int Size { get; set; }

	[JsonPropertyName("url")]
	public string Url { get; set; }

	[JsonPropertyName("id")]
	public string Id { get; set; }
}
