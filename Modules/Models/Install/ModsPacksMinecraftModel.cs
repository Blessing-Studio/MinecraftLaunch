using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MinecraftLaunch.Modules.Models.Install;

public class ModsPacksMinecraftModel
{
	[JsonPropertyName("version")]
	public string Version { get; set; }

	[JsonPropertyName("modLoaders")]
	public List<ModsPacksModLoaderModel> ModLoaders { get; set; }
}
