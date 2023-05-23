using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Linq;

namespace MinecraftLaunch.Modules.Models.Install;

public class FabricLauncherMeta
{
	[JsonPropertyName("mainClass")]
	public JToken MainClass { get; set; }

	[JsonPropertyName("libraries")]
	public Dictionary<string, List<FabricLibraryJsonEntity>> Libraries { get; set; }
}
