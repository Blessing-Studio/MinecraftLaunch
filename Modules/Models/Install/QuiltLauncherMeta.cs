using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace MinecraftLaunch.Modules.Models.Install;

public class QuiltLauncherMeta
{
	[JsonPropertyName("mainClass")]
	public JsonElement MainClass { get; set; }

	[JsonPropertyName("libraries")]
	public Dictionary<string, List<QuiltLibraryJsonEntity>> Libraries { get; set; }
}
