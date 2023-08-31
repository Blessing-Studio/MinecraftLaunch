using System.Collections.Generic;
using MinecraftLaunch.Modules.Models.Launch;
using System.Text.Json.Serialization;

namespace MinecraftLaunch.Modules.Models.Install;

public class OptiFineGameCoreJsonEntity
{
	[JsonPropertyName("arguments")]
	public ArgumentsJsonEntity Arguments { get; set; }

	[JsonPropertyName("id")]
	public string Id { get; set; }

	[JsonPropertyName("libraries")]
	public List<MinecraftLaunch.Modules.Models.Launch.LibraryJsonEntity> Libraries { get; set; }

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
