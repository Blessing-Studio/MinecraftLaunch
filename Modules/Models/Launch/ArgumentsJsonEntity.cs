using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MinecraftLaunch.Modules.Models.Launch;

public class ArgumentsJsonEntity
{
	[JsonProperty("game")]
	public List<JToken> Game { get; set; }

	[JsonProperty("jvm")]
	public List<JToken> Jvm { get; set; }
}
