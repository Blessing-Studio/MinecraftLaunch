using System.Collections.Generic;
using MinecraftLaunch.Modules.Models.Launch;
using Newtonsoft.Json;
using Newtonsoft.Json;

namespace MinecraftLaunch.Modules.Models.Download;

public class DownloadsJsonEntity
{
	[JsonProperty("artifact")]
	public FileJsonEntity Artifact { get; set; }

	[JsonProperty("classifiers")]
	public Dictionary<string, FileJsonEntity> Classifiers { get; set; }
}
