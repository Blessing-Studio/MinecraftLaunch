using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MinecraftLaunch.Modules.Models.Install;

public class FabricArgumentsJsonEntity
{
	[JsonProperty("jvm")]
	public List<JToken> Jvm { get; set; }
}
