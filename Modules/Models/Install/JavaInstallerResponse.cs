using MinecraftLaunch.Modules.Models.Launch;

namespace MinecraftLaunch.Modules.Models.Install;

public class JavaInstallerResponse : InstallerResponseBase
{
	public JavaInfo JavaInfo { get; set; }
}
