using Newtonsoft.Json;

namespace MinecraftLaunch.Modules.Models.Download;

public class ResourcePackInfo
{
	[JsonProperty("pack")]
	public ResourcePackDetailed pack { get; set; }
}
