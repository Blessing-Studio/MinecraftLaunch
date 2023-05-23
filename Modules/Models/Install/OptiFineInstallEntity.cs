using System.Text.Json.Serialization;

namespace MinecraftLaunch.Modules.Models.Install;

public class OptiFineInstallEntity
{
	[JsonPropertyName("patch")]
	public string Patch { get; set; }

	[JsonPropertyName("type")]
	public string Type { get; set; }

	[JsonPropertyName("mcversion")]
	public string McVersion { get; set; }

	[JsonPropertyName("filename")]
	public string FileName { get; set; }
}
