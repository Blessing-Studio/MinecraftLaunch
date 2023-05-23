using Newtonsoft.Json;

namespace MinecraftLaunch.Modules.Models.Launch;

public class AssetIndexJsonEntity : FileJsonEntity
{
	[JsonProperty("totalSize")]
	public int TotalSize { get; set; }
}
