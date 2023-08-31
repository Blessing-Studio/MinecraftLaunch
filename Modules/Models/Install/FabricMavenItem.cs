using System.Text.Json.Serialization;

namespace MinecraftLaunch.Modules.Models.Install;

public class FabricMavenItem
{
	[JsonPropertyName("separator")]
	public string Separator { get; set; }

	[JsonPropertyName("maven")]
	public string Maven { get; set; }

	[JsonPropertyName("version")]
	public string Version { get; set; }
}
