using Newtonsoft.Json;
using Newtonsoft.Json;

namespace MinecraftLaunch.Modules.Models.Download;

public class CurseForgeModpackCategory
{
	[JsonProperty("id")]
	public int Id { get; set; }

	[JsonProperty("name")]
	public string Name { get; set; }
}
