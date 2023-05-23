using System.Text.Json;
using System.Text.Json.Serialization;

namespace MinecraftLaunch.Modules.Models.Download;

internal class FabricMod
{
	public int schemaVersion { get; set; }

	public string id { get; set; }

	public string version { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

	public string description { get; set; }

	public JsonElement[] authors { get; set; }

	public FabricModContact contact { get; set; }
}
