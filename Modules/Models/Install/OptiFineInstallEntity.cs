using Newtonsoft.Json;

namespace MinecraftLaunch.Modules.Models.Install;

public class OptiFineInstallEntity
{
	[JsonProperty("patch")]
	public string Patch { get; set; }

	[JsonProperty("type")]
	public string Type { get; set; }

	[JsonProperty("mcversion")]
	public string McVersion { get; set; }

	[JsonProperty("filename")]
	public string FileName { get; set; }
}
