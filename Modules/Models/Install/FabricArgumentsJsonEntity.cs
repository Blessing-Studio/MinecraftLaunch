using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace MinecraftLaunch.Modules.Models.Install;

public class FabricArgumentsJsonEntity
{
	[JsonPropertyName("jvm")]
	public List<JsonElement> Jvm { get; set; }
}
