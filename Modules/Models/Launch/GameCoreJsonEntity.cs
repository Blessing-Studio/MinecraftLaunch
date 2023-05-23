using System.Collections.Generic;
using MinecraftLaunch.Modules.Interface;
using Newtonsoft.Json;

namespace MinecraftLaunch.Modules.Models.Launch;

public class GameCoreJsonEntity : IJsonEntity
{
	[JsonProperty("arguments")]
	public ArgumentsJsonEntity Arguments { get; set; }

	[JsonProperty("assetIndex")]
	public AssetIndexJsonEntity AssetIndex { get; set; }

	[JsonProperty("assets")]
	public string Assets { get; set; }

	[JsonProperty("javaVersion")]
	public JavaVersionJsonEntity JavaVersion { get; set; } = new JavaVersionJsonEntity
	{
		MajorVersion = 8
	};


	[JsonProperty("downloads")]
	public Dictionary<string, FileJsonEntity> Downloads { get; set; }

	[JsonProperty("id")]
	public string Id { get; set; }

	[JsonProperty("libraries")]
	public List<LibraryJsonEntity> Libraries { get; set; }

	[JsonProperty("logging")]
	public LoggingJsonEntity Logging { get; set; }

	[JsonProperty("minecraftArguments")]
	public string MinecraftArguments { get; set; }

	[JsonProperty("mainClass")]
	public string MainClass { get; set; }

	[JsonProperty("inheritsFrom")]
	public string InheritsFrom { get; set; }

	[JsonProperty("jar")]
	public string Jar { get; set; }

	[JsonProperty("minimumLauncherVersion")]
	public int? MinimumLauncherVersion { get; set; }

	[JsonProperty("releaseTime")]
	public string ReleaseTime { get; set; }

	[JsonProperty("time")]
	public string Time { get; set; }

	[JsonProperty("type")]
	public string Type { get; set; }
}
