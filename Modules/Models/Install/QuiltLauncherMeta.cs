using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MinecraftLaunch.Modules.Models.Install;

public class QuiltLauncherMeta
{
	[JsonProperty("mainClass")]
	public JToken MainClass { get; set; }

	[JsonProperty("libraries")]
	public Dictionary<string, List<QuiltLibraryJsonEntity>> Libraries { get; set; }
}
