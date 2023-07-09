using Newtonsoft.Json;

namespace MinecraftLaunch.Modules.Models.Download;

public class DataModel<T>
{
	[JsonProperty("data")]
	public T Data { get; set; }
}
