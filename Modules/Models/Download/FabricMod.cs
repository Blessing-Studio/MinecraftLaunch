using System.Text.Json;

namespace MinecraftLaunch.Modules.Models.Download;

internal class FabricMod
{
	public int schemaVersion { get; set; }

	public string id { get; set; }

	public string version { get; set; }

	public string name { get; set; }

	public string description { get; set; }

	public JsonElement[] authors { get; set; }

	public FabricModContact contact { get; set; }
}
