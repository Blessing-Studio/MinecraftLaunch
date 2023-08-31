using System.Collections.Generic;
using MinecraftLaunch.Modules.Interface;
using MinecraftLaunch.Modules.Parser;
using System.Text.Json.Serialization;

namespace MinecraftLaunch.Modules.Models.Install;

public class AssetJsonEntity : IJsonEntity
{
	[JsonPropertyName("objects")]
	public Dictionary<string, AssetsJsonEntity> Objects { get; set; }
}
