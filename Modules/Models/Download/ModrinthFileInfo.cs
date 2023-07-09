using System.Collections.Generic;
using Newtonsoft.Json;

namespace MinecraftLaunch.Modules.Models.Download;

public class ModrinthFileInfo
{
	[JsonProperty("hashes")]
	public Dictionary<string, string> Hashes { get; set; }

	[JsonProperty("url")]
	public string Url { get; set; }

	[JsonProperty("filename")]
	public string FileName { get; set; }

	[JsonProperty("primary")]
	public bool Primary { get; set; }
}
