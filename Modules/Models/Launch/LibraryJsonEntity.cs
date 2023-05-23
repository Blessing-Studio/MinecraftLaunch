using System.Collections.Generic;
using MinecraftLaunch.Modules.Models.Download;
using System.Text.Json.Serialization;

namespace MinecraftLaunch.Modules.Models.Launch;

public class LibraryJsonEntity
{
	[JsonPropertyName("downloads")]
	public DownloadsJsonEntity Downloads { get; set; }

	[JsonPropertyName("name")]
	public string Name { get; set; }

	[JsonPropertyName("url")]
	public string Url { get; set; }

	[JsonPropertyName("natives")]
	public Dictionary<string, string> Natives { get; set; }

	[JsonPropertyName("rules")]
	public IEnumerable<RuleEntity> Rules { get; set; }

	[JsonPropertyName("checksums")]
	public List<string> CheckSums { get; set; }

	[JsonPropertyName("clientreq")]
	public bool? ClientReq { get; set; }
}
