using System.Collections.Generic;
using MinecraftLaunch.Modules.Models.Download;
using Newtonsoft.Json;

namespace MinecraftLaunch.Modules.Models.Launch;

public class LibraryJsonEntity
{
	[JsonProperty("downloads")]
	public DownloadsJsonEntity Downloads { get; set; }

	[JsonProperty("name")]
	public string Name { get; set; }

	[JsonProperty("url")]
	public string Url { get; set; }

	[JsonProperty("natives")]
	public Dictionary<string, string> Natives { get; set; }

	[JsonProperty("rules")]
	public IEnumerable<RuleEntity> Rules { get; set; }

	[JsonProperty("checksums")]
	public List<string> CheckSums { get; set; }

	[JsonProperty("clientreq")]
	public bool? ClientReq { get; set; }
}
