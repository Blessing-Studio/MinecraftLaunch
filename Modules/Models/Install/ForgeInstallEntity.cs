using System;
using System.Text.Json.Serialization;

namespace MinecraftLaunch.Modules.Models.Install;

public class ForgeInstallEntity
{
	[JsonPropertyName("branch")]
	public string Branch { get; set; }

	[JsonPropertyName("build")]
	public int Build { get; set; }

	[JsonPropertyName("mcversion")]
	public string McVersion { get; set; }

	[JsonPropertyName("version")]
	public string ForgeVersion { get; set; }

	[JsonPropertyName("modified")]
	public DateTime ModifiedTime { get; set; }
}
