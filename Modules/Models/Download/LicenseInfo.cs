using Newtonsoft.Json;

namespace MinecraftLaunch.Modules.Models.Download;

public class LicenseInfo
{
	[JsonProperty("id")]
	public string Id { get; set; }

	[JsonProperty("name")]
	public string Name { get; set; }

	[JsonProperty("url")]
	public string Url { get; set; }
}
