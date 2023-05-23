using System;

namespace MinecraftLaunch.Modules.Models.Download;

[Obsolete]
public class SearchOptions
{
	public int? CategoryId { get; set; }

	public int? GameId { get; set; }

	public string? GameVersion { get; set; }

	public int? Index { get; set; }

	public int? PageSize { get; set; }

	public string? SearchFilter { get; set; }

	public int? Sort { get; set; }

	public SearchOptions(string? gameVersion, int? index, int? pageSize, string? searchFilter, int? sort, int? gameId = 432, int? categoryId = -1)
	{
		CategoryId = categoryId;
		GameId = gameId;
		GameVersion = gameVersion;
		Index = index;
		PageSize = pageSize;
		SearchFilter = searchFilter;
		Sort = sort;
	}
}
