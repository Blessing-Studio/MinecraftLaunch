using System.Collections.Generic;
using Newtonsoft.Json;

namespace MinecraftLaunch.Modules.Models.Download;

public class ModrinthSearchResponse
{
	[JsonProperty("hits")]
	public List<ModrinthProjectInfoSearchResult> Hits { get; set; }

	[JsonProperty("offset")]
	public int Offset { get; set; }

	[JsonProperty("limit")]
	public int Limit { get; set; }

	[JsonProperty("total_hits")]
	public int TotalHits { get; set; }
}
