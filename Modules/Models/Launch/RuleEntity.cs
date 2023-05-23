using System.Collections.Generic;
using Newtonsoft.Json;

namespace MinecraftLaunch.Modules.Models.Launch;

public class RuleEntity
{
	[JsonProperty("action")]
	public string Action { get; set; }

	[JsonProperty("os")]
	public Dictionary<string, string> System { get; set; }
}
