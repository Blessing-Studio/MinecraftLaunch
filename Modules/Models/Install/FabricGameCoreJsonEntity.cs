using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MinecraftLaunch.Modules.Models.Install;

public class FabricGameCoreJsonEntity
{
	[JsonPropertyName("arguments")]
	public FabricArgumentsJsonEntity Arguments { get; set; }

	[JsonPropertyName("id")]
	public string Id { get; set; }

	[JsonPropertyName("libraries")]
	public List<FabricLibraryJsonEntity>? Libraries { get; set; }

	[JsonPropertyName("mainClass")]
	public string MainClass { get; set; }

	[JsonPropertyName("inheritsFrom")]
	public string InheritsFrom { get; set; }

	[JsonPropertyName("releaseTime")]
	public string ReleaseTime { get; set; }

	[JsonPropertyName("time")]
	public string Time { get; set; }

	[JsonPropertyName("type")]
	public string Type { get; set; }
}
