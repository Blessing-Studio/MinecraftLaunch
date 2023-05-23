using Newtonsoft.Json;

namespace MinecraftLaunch.Modules.Models.Install;

public class ModsPacksFileModel
{
	[JsonProperty("projectID")]
	public long ProjectId { get; set; }

	[JsonProperty("fileID")]
	public long FileId { get; set; }

	[JsonProperty("required")]
	public bool Required { get; set; }
}
