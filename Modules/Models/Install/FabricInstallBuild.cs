using Newtonsoft.Json;

namespace MinecraftLaunch.Modules.Models.Install;

public class FabricInstallBuild
{
	[JsonProperty("intermediary")]
	public FabricMavenItem Intermediary { get; set; }

	[JsonProperty("loader")]
	public FabricMavenItem Loader { get; set; }

	[JsonProperty("launcherMeta")]
	public FabricLauncherMeta LauncherMeta { get; set; }
}
