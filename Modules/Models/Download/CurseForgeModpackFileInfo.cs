using MinecraftLaunch.Modules.Enum;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization;

namespace MinecraftLaunch.Modules.Models.Download;

public class CurseForgeModpackFileInfo
{
	public string DownloadUrl { get; set; }

	[JsonPropertyName("fileId")]
	public int FileId { get; set; }

	[JsonPropertyName("filename")]
	public string FileName { get; set; }

	[JsonPropertyName("modLoader")]
	public ModLoaderType? ModLoaderType { get; set; }

	[JsonPropertyName("gameVersion")]
	public string SupportedVersion { get; set; }
}
