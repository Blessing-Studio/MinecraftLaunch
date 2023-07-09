using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MinecraftLaunch.Modules.Models.Install;

public class FabricLauncherMeta
{
	[JsonProperty("mainClass")]
	public JToken MainClass { get; set; }

	[JsonProperty("libraries")]
	public Dictionary<string, List<FabricLibraryJsonEntity>> Libraries { get; set; }
}
