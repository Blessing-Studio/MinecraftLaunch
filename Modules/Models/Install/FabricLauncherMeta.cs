using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace MinecraftLaunch.Modules.Models.Install;

public class FabricLauncherMeta
{
	[JsonPropertyName("mainClass")]
	public JsonElement MainClass { get; set; }

	[JsonPropertyName("libraries")]
	public Dictionary<string, List<FabricLibraryJsonEntity>> Libraries { get; set; }
}
