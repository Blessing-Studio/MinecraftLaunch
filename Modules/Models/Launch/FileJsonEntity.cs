using Newtonsoft.Json;

namespace MinecraftLaunch.Modules.Models.Launch;

public class FileJsonEntity
{
	[JsonProperty("path")]
	public string Path { get; set; }

	[JsonProperty("sha1")]
	public string Sha1 { get; set; }

	[JsonProperty("size")]
	public int Size { get; set; }

	[JsonProperty("url")]
	public string Url { get; set; }

	[JsonProperty("id")]
	public string Id { get; set; }
}
