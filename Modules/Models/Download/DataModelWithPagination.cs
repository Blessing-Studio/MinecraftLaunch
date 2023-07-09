using Newtonsoft.Json;
using Newtonsoft.Json;

namespace MinecraftLaunch.Modules.Models.Download;

public class DataModelWithPagination<T> : DataModel<T>
{
	[JsonProperty("pagination")]
	public PaginationModel Pagination { get; set; }
}
