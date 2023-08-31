using System.Text.Json.Serialization;

namespace MinecraftLaunch.Modules.Models.Download;

public class ResourcePackDetailed
{
	[JsonPropertyName("pack_format")]
	public int pack_format { get; set; }

	[JsonPropertyName("description")]
	public string description { get; set; }
}
