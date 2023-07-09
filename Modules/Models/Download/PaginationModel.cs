using Newtonsoft.Json;

namespace MinecraftLaunch.Modules.Models.Download;

public class PaginationModel
{
	[JsonProperty("index")]
	public int Index { get; set; }

	[JsonProperty("pageSize")]
	public int PageSize { get; set; }

	[JsonProperty("resultCount")]
	public int ResultCount { get; set; }

	[JsonProperty("totalCount")]
	public int TotalCount { get; set; }
}
