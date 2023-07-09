using Newtonsoft.Json;

namespace MinecraftLaunch.Modules.Models.Install;

public class QuiltLibraryJsonEntity
{
	[JsonProperty("name")]
	public string Name { get; set; }

	[JsonProperty("url")]
	public string Url { get; set; }
}
