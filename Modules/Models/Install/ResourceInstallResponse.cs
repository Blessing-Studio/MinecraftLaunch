using System.Collections.Generic;
using MinecraftLaunch.Modules.Interface;

namespace MinecraftLaunch.Modules.Models.Install;

public class ResourceInstallResponse
{
	public int Total { get; set; }

	public int SuccessCount { get; set; }

	public List<IResource> FailedResources { get; set; }
}
