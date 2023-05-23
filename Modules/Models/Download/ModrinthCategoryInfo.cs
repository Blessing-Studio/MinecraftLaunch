using System.Text.Json.Serialization;

namespace MinecraftLaunch.Modules.Models.Download;

public class ModrinthCategoryInfo
{
	[JsonPropertyName("icon")]
	public string Icon { get; set; }

	[JsonPropertyName("name")]
	public string Name { get; set; }

	[JsonPropertyName("project_type")]
	public string ProjectType { get; set; }
}
