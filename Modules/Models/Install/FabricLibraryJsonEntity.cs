using Newtonsoft.Json;

namespace MinecraftLaunch.Modules.Models.Install;

public class FabricLibraryJsonEntity
{
	[JsonProperty("name")]
	public string Name { get; set; }

	[JsonProperty("url")]
	public string Url { get; set; }
}
