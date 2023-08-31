using System.Text.Json.Serialization;

namespace MinecraftLaunch.Modules.Models.Download;

public class ResourcePackInfo
{
	[JsonPropertyName("pack")]
	public ResourcePackDetailed pack { get; set; }
}
