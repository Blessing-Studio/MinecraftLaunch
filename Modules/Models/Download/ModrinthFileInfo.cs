using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MinecraftLaunch.Modules.Models.Download;

public class ModrinthFileInfo
{
	[JsonPropertyName("hashes")]
	public Dictionary<string, string> Hashes { get; set; }

	[JsonPropertyName("url")]
	public string Url { get; set; }

	[JsonPropertyName("filename")]
	public string FileName { get; set; }

	[JsonPropertyName("primary")]
	public bool Primary { get; set; }
}
