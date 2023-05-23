using System.Text.Json.Serialization;

namespace MinecraftLaunch.Modules.Models.Download;

public class LicenseInfo
{
	[JsonPropertyName("id")]
	public string Id { get; set; }

	[JsonPropertyName("name")]
	public string Name { get; set; }

	[JsonPropertyName("url")]
	public string Url { get; set; }
}
