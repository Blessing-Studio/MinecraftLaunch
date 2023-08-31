using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace MinecraftLaunch.Modules.Models.Install;

public class QuiltArgumentsJsonEntity
{
	[JsonPropertyName("game")]
	public List<JsonElement> Game { get; set; }
}
