using Newtonsoft.Json;

namespace MinecraftLaunch.Modules.Models.Launch;

public class ClientJsonEntity
{
	[JsonProperty("argument")]
	public string Argument { get; set; }

	[JsonProperty("file")]
	public FileJsonEntity File { get; set; }

	[JsonProperty("type")]
	public string Type { get; set; }
}
