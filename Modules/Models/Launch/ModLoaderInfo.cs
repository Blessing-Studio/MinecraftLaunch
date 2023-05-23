using MinecraftLaunch.Modules.Enum;

namespace MinecraftLaunch.Modules.Models.Launch;

public class ModLoaderInfo
{
	public ModLoaderType ModLoaderType { get; set; }

	public string? Version { get; set; }
}
