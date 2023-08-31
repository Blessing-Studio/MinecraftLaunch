using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MinecraftLaunch.Modules.Models.Install;

public class ForgeInstallProcessorEntity
{
	[JsonPropertyName("sides")]
	public List<string> Sides { get; set; } = new List<string>();


	[JsonPropertyName("jar")]
	public string Jar { get; set; }

	[JsonPropertyName("classpath")]
	public List<string> Classpath { get; set; }

	[JsonPropertyName("args")]
	public List<string> Args { get; set; }

	[JsonPropertyName("outputs")]
	public Dictionary<string, string> Outputs { get; set; } = new Dictionary<string, string>();

}
