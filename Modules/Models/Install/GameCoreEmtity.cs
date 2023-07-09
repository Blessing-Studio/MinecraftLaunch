using System;
using Newtonsoft.Json;

namespace MinecraftLaunch.Modules.Models.Install;

public class GameCoreEmtity
{
	[JsonProperty("id")]
	public string Id { get; set; }

	[JsonProperty("type")]
	public string Type { get; set; }

	[JsonProperty("url")]
	public string Url { get; set; }

	[JsonProperty("time")]
	public DateTime Time { get; set; }

	[JsonProperty("releaseTime")]
	public DateTime ReleaseTime { get; set; }
}
