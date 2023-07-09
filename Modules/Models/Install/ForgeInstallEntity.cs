using System;
using Newtonsoft.Json;

namespace MinecraftLaunch.Modules.Models.Install;

public class ForgeInstallEntity
{
	[JsonProperty("branch")]
	public string Branch { get; set; }

	[JsonProperty("build")]
	public int Build { get; set; }

	[JsonProperty("mcversion")]
	public string McVersion { get; set; }

	[JsonProperty("version")]
	public string ForgeVersion { get; set; }

	[JsonProperty("modified")]
	public DateTime ModifiedTime { get; set; }
}
