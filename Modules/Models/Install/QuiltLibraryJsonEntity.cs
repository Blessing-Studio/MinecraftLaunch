using System.Text.Json.Serialization;

namespace MinecraftLaunch.Modules.Models.Install;

public class QuiltLibraryJsonEntity
{
	[JsonPropertyName("name")]
	public string Name { get; set; }

	[JsonPropertyName("url")]
	public string Url { get; set; }
}
