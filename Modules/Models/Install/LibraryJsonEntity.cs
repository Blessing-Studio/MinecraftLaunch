using System.Text.Json.Serialization;

namespace MinecraftLaunch.Modules.Models.Install;

public class LibraryJsonEntity
{
	[JsonPropertyName("name")]
	public string Name { get; set; }
}
