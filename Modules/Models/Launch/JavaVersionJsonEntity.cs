using Newtonsoft.Json;

namespace MinecraftLaunch.Modules.Models.Launch;

public class JavaVersionJsonEntity
{
	[JsonProperty("component")]
	public string Component { get; set; }

	[JsonProperty("majorVersion")]
	public int MajorVersion { get; set; }
}
