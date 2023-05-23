using System;

namespace MinecraftLaunch.Modules.Models.Install;

public class InstallerResponseBase
{
	public Exception Exception { get; set; }

	public bool Success { get; set; }
}
