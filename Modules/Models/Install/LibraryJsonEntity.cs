using Newtonsoft.Json;

namespace MinecraftLaunch.Modules.Models.Install;

public class LibraryJsonEntity
{
	[JsonProperty("name")]
	public string Name { get; set; }
}
