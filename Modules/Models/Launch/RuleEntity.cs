using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MinecraftLaunch.Modules.Models.Launch;

public class RuleEntity
{
	[JsonPropertyName("action")]
	public string Action { get; set; }

	[JsonPropertyName("os")]
	public Dictionary<string, string> System { get; set; }
}
