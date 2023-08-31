using System.Collections.Generic;
using MinecraftLaunch.Modules.Interface;
using System.Text.Json.Serialization;

namespace MinecraftLaunch.Modules.Models.Launch;

public class GameCoreJsonEntity : IJsonEntity
{
	[JsonPropertyName("arguments")]
	public ArgumentsJsonEntity Arguments { get; set; }

	[JsonPropertyName("assetIndex")]
	public AssetIndexJsonEntity AssetIndex { get; set; }

	[JsonPropertyName("assets")]
	public string Assets { get; set; }

	[JsonPropertyName("javaVersion")]
	public JavaVersionJsonEntity JavaVersion { get; set; } = new JavaVersionJsonEntity
	{
		MajorVersion = 8
	};


	[JsonPropertyName("downloads")]
	public Dictionary<string, FileJsonEntity> Downloads { get; set; }

	[JsonPropertyName("id")]
	public string Id { get; set; }

	[JsonPropertyName("libraries")]
	public List<LibraryJsonEntity> Libraries { get; set; }

	[JsonPropertyName("logging")]
	public LoggingJsonEntity Logging { get; set; }

	[JsonPropertyName("minecraftArguments")]
	public string MinecraftArguments { get; set; }

	[JsonPropertyName("mainClass")]
	public string MainClass { get; set; }

	[JsonPropertyName("inheritsFrom")]
	public string InheritsFrom { get; set; }

	[JsonPropertyName("jar")]
	public string Jar { get; set; }

	[JsonPropertyName("minimumLauncherVersion")]
	public int? MinimumLauncherVersion { get; set; }

	[JsonPropertyName("releaseTime")]
	public string ReleaseTime { get; set; }

	[JsonPropertyName("time")]
	public string Time { get; set; }

	[JsonPropertyName("type")]
	public string Type { get; set; }
}
