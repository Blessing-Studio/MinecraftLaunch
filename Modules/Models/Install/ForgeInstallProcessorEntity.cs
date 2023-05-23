using System.Collections.Generic;
using Newtonsoft.Json;

namespace MinecraftLaunch.Modules.Models.Install;

public class ForgeInstallProcessorEntity
{
	[JsonProperty("sides")]
	public List<string> Sides { get; set; } = new List<string>();


	[JsonProperty("jar")]
	public string Jar { get; set; }

	[JsonProperty("classpath")]
	public List<string> Classpath { get; set; }

	[JsonProperty("args")]
	public List<string> Args { get; set; }

	[JsonProperty("outputs")]
	public Dictionary<string, string> Outputs { get; set; } = new Dictionary<string, string>();

}
