using System.Collections.Generic;
using Newtonsoft.Json;

namespace MinecraftLaunch.Modules.Models.Install;

public class FabricGameCoreJsonEntity
{
	[JsonProperty("arguments")]
	public FabricArgumentsJsonEntity Arguments { get; set; }

	[JsonProperty("id")]
	public string Id { get; set; }

	[JsonProperty("libraries")]
	public List<FabricLibraryJsonEntity>? Libraries { get; set; }

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
