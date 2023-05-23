using Newtonsoft.Json;

namespace MinecraftLaunch.Modules.Models.Install;

public class ModsPacksModLoaderModel
{
	[JsonProperty("id")]
	public string Id { get; set; }

	[JsonProperty("primary")]
	public bool IsPrimary { get; set; }
}
