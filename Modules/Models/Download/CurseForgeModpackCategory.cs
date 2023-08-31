using System.Text.Json.Serialization;
using System.Text.Json.Serialization;

namespace MinecraftLaunch.Modules.Models.Download;

public class CurseForgeModpackCategory
{
	[JsonPropertyName("id")]
	public int Id { get; set; }

	[JsonPropertyName("name")]
	public string Name { get; set; }
}
