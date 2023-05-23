using Newtonsoft.Json;

namespace MinecraftLaunch.Modules.Models.Install;

public class QuiltInstallBuild
{
	[JsonProperty("intermediary")]
	public QuiltMavenItem Intermediary { get; set; }

	[JsonProperty("loader")]
	public QuiltMavenItem Loader { get; set; }

	[JsonProperty("launcherMeta")]
	public QuiltLauncherMeta LauncherMeta { get; set; }

	[JsonProperty("hashed")]
	public QuiltMavenItem Hashed { get; set; }
}
