using System.Text.Json.Serialization;

namespace MinecraftLaunch.Modules.Models.Launch;

public class JavaVersionJsonEntity
{
	[JsonPropertyName("component")]
	public string Component { get; set; }

	[JsonPropertyName("majorVersion")]
	public int MajorVersion { get; set; }
}
