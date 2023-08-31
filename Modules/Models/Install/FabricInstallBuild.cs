using System.Text.Json.Serialization;

namespace MinecraftLaunch.Modules.Models.Install;

public class FabricInstallBuild
{
	[JsonPropertyName("intermediary")]
	public FabricMavenItem Intermediary { get; set; }

	[JsonPropertyName("loader")]
	public FabricMavenItem Loader { get; set; }

	[JsonPropertyName("launcherMeta")]
	public FabricLauncherMeta LauncherMeta { get; set; }
}
