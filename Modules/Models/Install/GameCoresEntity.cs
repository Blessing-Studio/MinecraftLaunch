using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MinecraftLaunch.Modules.Models.Install;

public class GameCoresEntity
{
	[JsonPropertyName("latest")]
	public Dictionary<string, string> Latest { get; set; }

	[JsonPropertyName("versions")]
	public IEnumerable<GameCoreEmtity> Cores { get; set; }
}
