using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MinecraftLaunch.Modules.Models.Install;

public class ArgumentsJsonEntity
{
	[JsonProperty("game")]
	public List<JToken> Game { get; set; }
}
