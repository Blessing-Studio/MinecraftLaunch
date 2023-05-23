using Newtonsoft.Json;

namespace MinecraftLaunch.Modules.Models.Download;

public class ModrinthDependency
{
	[JsonProperty("version_id")]
	public string VersionId { get; set; }

	[JsonProperty("project_id")]
	public string ProjectId { get; set; }

	[JsonProperty("dependency_type")]
	public string DependencyType { get; set; }
}
