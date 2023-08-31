using System.Text.Json.Serialization;

namespace MinecraftLaunch.Modules.Models.Launch;

public class AssetIndexJsonEntity : FileJsonEntity
{
	[JsonPropertyName("totalSize")]
	public int TotalSize { get; set; }
}
