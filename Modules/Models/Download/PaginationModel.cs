using System.Text.Json.Serialization;

namespace MinecraftLaunch.Modules.Models.Download;

public class PaginationModel
{
	[JsonPropertyName("index")]
	public int Index { get; set; }

	[JsonPropertyName("pageSize")]
	public int PageSize { get; set; }

	[JsonPropertyName("resultCount")]
	public int ResultCount { get; set; }

	[JsonPropertyName("totalCount")]
	public int TotalCount { get; set; }
}
