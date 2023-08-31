using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MinecraftLaunch.Modules.Models.Install;

public class ModsPacksModel
{
	[JsonPropertyName("minecraft")]
	public ModsPacksMinecraftModel Minecraft { get; set; }

	[JsonPropertyName("manifestType")]
	public string ManifestType { get; set; }

	[JsonPropertyName("manifestVersion")]
	public int ManifestVersion { get; set; }

	[JsonPropertyName("name")]
	public string Name { get; set; }

	[JsonPropertyName("version")]
	public string Version { get; set; }

	[JsonPropertyName("author")]
	public string Author { get; set; }

	[JsonPropertyName("files")]
	public List<ModsPacksFileModel> Files { get; set; }

	[JsonPropertyName("overrides")]
	public string Overrides { get; set; }
}
