namespace MinecraftLaunch.Modules.Models.Download;

public class ModPack
{
	public string Id { get; set; }

	public string FileName { get; set; }

	public string DisplayName { get; set; }

	public string Description { get; set; }

	public string Version { get; set; }

	public string GameVersion { get; set; }

	public string Authors { get; set; }

	public string Url { get; set; }

	public string Path { get; set; }

	public bool IsEnabled { get; set; }
}
