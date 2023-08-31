using System.Text.Json.Serialization;

namespace MinecraftLaunch.Modules.Models.Launch;

public class LoggingJsonEntity
{
	[JsonPropertyName("client")]
	public ClientJsonEntity Client { get; set; }
}
