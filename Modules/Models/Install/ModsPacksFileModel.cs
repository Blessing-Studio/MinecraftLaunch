using System.Text.Json.Serialization;

namespace MinecraftLaunch.Modules.Models.Install;

public class ModsPacksFileModel
{
	[JsonPropertyName("projectID")]
	public long ProjectId { get; set; }

	[JsonPropertyName("fileID")]
	public long FileId { get; set; }

	[JsonPropertyName("required")]
	public bool Required { get; set; }
}
