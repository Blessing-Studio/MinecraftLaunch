using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MinecraftLaunch.Modules.Models.Download;

public class ModrinthSearchResponse
{
	[JsonPropertyName("hits")]
	public List<ModrinthProjectInfoSearchResult> Hits { get; set; }

	[JsonPropertyName("offset")]
	public int Offset { get; set; }

	[JsonPropertyName("limit")]
	public int Limit { get; set; }

	[JsonPropertyName("total_hits")]
	public int TotalHits { get; set; }
}
