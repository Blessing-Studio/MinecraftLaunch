using System.Collections.Generic;
using MinecraftLaunch.Modules.Models.Launch;
using Newtonsoft.Json;

namespace MinecraftLaunch.Modules.Models.Install;

public class OptiFineGameCoreJsonEntity
{
	[JsonProperty("arguments")]
	public ArgumentsJsonEntity Arguments { get; set; }

	[JsonProperty("id")]
	public string Id { get; set; }

	[JsonProperty("libraries")]
	public List<MinecraftLaunch.Modules.Models.Launch.LibraryJsonEntity> Libraries { get; set; }

	[JsonProperty("mainClass")]
	public string MainClass { get; set; }

	[JsonProperty("inheritsFrom")]
	public string InheritsFrom { get; set; }

	[JsonProperty("releaseTime")]
	public string ReleaseTime { get; set; }

	[JsonProperty("time")]
	public string Time { get; set; }

	[JsonProperty("type")]
	public string Type { get; set; }
}
