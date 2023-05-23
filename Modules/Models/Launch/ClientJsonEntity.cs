using System.Text.Json.Serialization;

namespace MinecraftLaunch.Modules.Models.Launch;

public class ClientJsonEntity
{
	[JsonPropertyName("argument")]
	public string Argument { get; set; }

	[JsonPropertyName("file")]
	public FileJsonEntity File { get; set; }

	[JsonPropertyName("type")]
	public string Type { get; set; }
}
