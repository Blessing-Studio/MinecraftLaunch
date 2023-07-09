using Newtonsoft.Json;

namespace MinecraftLaunch.Modules.Parser;

public class AssetsJsonEntity
{
	[JsonProperty("hash")]
	public string Hash { get; set; }

	[JsonProperty("size")]
	public int Size { get; set; }
}
