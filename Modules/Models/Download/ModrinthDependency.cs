using System.Text.Json.Serialization;

namespace MinecraftLaunch.Modules.Models.Download;

public class ModrinthDependency
{
	[JsonPropertyName("version_id")]
	public string VersionId { get; set; }

	[JsonPropertyName("project_id")]
	public string ProjectId { get; set; }

	[JsonPropertyName("dependency_type")]
	public string DependencyType { get; set; }
}
