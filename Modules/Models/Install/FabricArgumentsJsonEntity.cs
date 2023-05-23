using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Linq;

namespace MinecraftLaunch.Modules.Models.Install;

public class FabricArgumentsJsonEntity
{
	[JsonPropertyName("jvm")]
	public List<JToken> Jvm { get; set; }
}
