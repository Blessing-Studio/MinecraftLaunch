using Newtonsoft.Json;

namespace MinecraftLaunch.Modules.Models.Launch;

public class LoggingJsonEntity
{
	[JsonProperty("client")]
	public ClientJsonEntity Client { get; set; }
}
