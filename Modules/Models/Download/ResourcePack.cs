using System.IO;

namespace MinecraftLaunch.Modules.Models.Download;

public class ResourcePack
{
	public string Id { get; set; }

	public int Format { get; set; }

	public string Description { get; set; }

	public Stream ImageStream { get; set; }

	public string Path { get; set; }

	public bool IsEnabled { get; set; }

	public bool IsExtracted { get; set; }
}
