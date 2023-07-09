using Newtonsoft.Json;

namespace MinecraftLaunch.Modules.Models.Download;

public class ModrinthCategoryInfo
{
	[JsonProperty("icon")]
	public string Icon { get; set; }

	[JsonProperty("name")]
	public string Name { get; set; }

	[JsonProperty("project_type")]
	public string ProjectType { get; set; }
}
