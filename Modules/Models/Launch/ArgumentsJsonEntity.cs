using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MinecraftLaunch.Modules.Models.Launch;

public class ArgumentsJsonEntity
{
	[JsonPropertyName("game")]
	public List<object> Game { get; set; }

	[JsonPropertyName("jvm")]
	public List<object> Jvm { get; set; }
}
