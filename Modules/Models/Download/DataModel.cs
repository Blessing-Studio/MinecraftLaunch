using System.Text.Json.Serialization;

namespace MinecraftLaunch.Modules.Models.Download;

public class DataModel<T>
{
	[JsonPropertyName("data")]
	public T Data { get; set; }
}
