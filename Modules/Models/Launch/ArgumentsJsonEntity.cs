using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MinecraftLaunch.Modules.Models.Launch;

public class ArgumentsJsonEntity
{
	[JsonProperty("game")]
	public List<object> Game { get; set; }

	[JsonProperty("jvm")]
	public List<object> Jvm { get; set; }
}
