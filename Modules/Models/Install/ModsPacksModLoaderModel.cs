using System.Text.Json.Serialization;

namespace MinecraftLaunch.Modules.Models.Install;

public class ModsPacksModLoaderModel
{
	[JsonPropertyName("id")]
	public string Id { get; set; }

	[JsonPropertyName("primary")]
	public bool IsPrimary { get; set; }
}
