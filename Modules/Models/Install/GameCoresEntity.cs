using System.Collections.Generic;
using Newtonsoft.Json;

namespace MinecraftLaunch.Modules.Models.Install;

public class GameCoresEntity
{
	[JsonProperty("latest")]
	public Dictionary<string, string> Latest { get; set; }

	[JsonProperty("versions")]
	public IEnumerable<GameCoreEmtity> Cores { get; set; }
}
