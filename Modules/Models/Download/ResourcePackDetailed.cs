using Newtonsoft.Json;

namespace MinecraftLaunch.Modules.Models.Download;

public class ResourcePackDetailed
{
	[JsonProperty("pack_format")]
	public int pack_format { get; set; }

	[JsonProperty("description")]
	public string description { get; set; }
}
