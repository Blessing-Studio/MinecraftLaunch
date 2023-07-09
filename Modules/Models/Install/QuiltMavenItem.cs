using Newtonsoft.Json;

namespace MinecraftLaunch.Modules.Models.Install;

public class QuiltMavenItem
{
	[JsonProperty("separator")]
	public string Separator { get; set; }

	[JsonProperty("maven")]
	public string Maven { get; set; }

	[JsonProperty("version")]
	public string Version { get; set; }

	[JsonProperty("build")]
	public int build { get; set; }
}
