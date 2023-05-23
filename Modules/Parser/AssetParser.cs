using System.Collections.Generic;
using System.IO;
using MinecraftLaunch.Modules.Models.Download;
using MinecraftLaunch.Modules.Models.Install;

namespace MinecraftLaunch.Modules.Parser;

public class AssetParser
{
	public AssetJsonEntity Entity { get; set; }

	public DirectoryInfo Root { get; set; }

	public AssetParser(AssetJsonEntity jsonEntity, DirectoryInfo directoryInfo)
	{
		Entity = jsonEntity;
		Root = directoryInfo;
	}

	public IEnumerable<AssetResource> GetAssets()
	{
		foreach (KeyValuePair<string, AssetsJsonEntity> @object in Entity.Objects)
		{
			yield return new AssetResource
			{
				Name = @object.Key,
				CheckSum = @object.Value.Hash,
				Size = @object.Value.Size,
				Root = Root
			};
		}
	}
}
