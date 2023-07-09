using System.Collections.Generic;
using Newtonsoft.Json;

namespace MinecraftLaunch.Modules.Models.Install;

public class ModsPacksMinecraftModel
{
	[JsonProperty("version")]
	public string Version { get; set; }

	[JsonProperty("modLoaders")]
	public List<ModsPacksModLoaderModel> ModLoaders { get; set; }
}
