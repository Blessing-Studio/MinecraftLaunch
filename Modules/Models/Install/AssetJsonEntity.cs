using System.Collections.Generic;
using MinecraftLaunch.Modules.Interface;
using MinecraftLaunch.Modules.Parser;
using Newtonsoft.Json;

namespace MinecraftLaunch.Modules.Models.Install;

public class AssetJsonEntity : IJsonEntity
{
	[JsonProperty("objects")]
	public Dictionary<string, AssetsJsonEntity> Objects { get; set; }
}
