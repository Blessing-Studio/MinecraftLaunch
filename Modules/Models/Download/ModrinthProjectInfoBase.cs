using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MinecraftLaunch.Modules.Models.Download;

public class ModrinthProjectInfoBase
{
	[JsonPropertyName("project_type")]
	public string ProjectType { get; set; }

	[JsonPropertyName("slug")]
	public string Slug { get; set; }

	[JsonPropertyName("title")]
	public string Title { get; set; }

	[JsonPropertyName("description")]
	public string Description { get; set; }

	[JsonPropertyName("categories")]
	public List<string> Categories { get; set; }

	[JsonPropertyName("versions")]
	public List<string> Versions { get; set; }

	[JsonPropertyName("downloads")]
	public int Downloads { get; set; }

	[JsonPropertyName("icon_url")]
	public string IconUrl { get; set; }

	[JsonPropertyName("client_side")]
	public string ClientSide { get; set; }

	[JsonPropertyName("server_side")]
	public string ServerSide { get; set; }

	[JsonPropertyName("gallery")]
	public List<object> Gallery { get; set; }
}
