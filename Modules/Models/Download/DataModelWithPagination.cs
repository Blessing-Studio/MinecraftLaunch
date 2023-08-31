using System.Text.Json.Serialization;
using System.Text.Json.Serialization;

namespace MinecraftLaunch.Modules.Models.Download;

public class DataModelWithPagination<T> : DataModel<T>
{
	[JsonPropertyName("pagination")]
	public PaginationModel Pagination { get; set; }
}
