
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MinecraftLaunch.Modules.Models.Launch;

public class ArgumentsJsonEntity
{
	[JsonPropertyName("game")]
	public List<JsonElement> Game { get; set; }

	[JsonPropertyName("jvm")]
	public List<JsonElement> Jvm { get; set; }
}
