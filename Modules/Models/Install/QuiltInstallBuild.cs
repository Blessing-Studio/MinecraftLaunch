using System.Text.Json.Serialization;

namespace MinecraftLaunch.Modules.Models.Install;

public class QuiltInstallBuild
{
	[JsonPropertyName("intermediary")]
	public QuiltMavenItem Intermediary { get; set; }

	[JsonPropertyName("loader")]
	public QuiltMavenItem Loader { get; set; }

	[JsonPropertyName("launcherMeta")]
	public QuiltLauncherMeta LauncherMeta { get; set; }

	[JsonPropertyName("hashed")]
	public QuiltMavenItem Hashed { get; set; }
}
