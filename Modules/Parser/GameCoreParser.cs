using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MinecraftLaunch.Modules.Enum;
using MinecraftLaunch.Modules.Models.Download;
using MinecraftLaunch.Modules.Models.Launch;
using MinecraftLaunch.Modules.Toolkits;
using Newtonsoft.Json.Linq;

namespace MinecraftLaunch.Modules.Parser;

public class GameCoreParser
{
	public DirectoryInfo Root { get; set; }

	public IEnumerable<GameCoreJsonEntity> JsonEntities { get; set; }

	public List<(string, Exception)> ErrorGameCores { get; private set; } = new List<(string, Exception)>();


	public GameCoreParser(DirectoryInfo root, IEnumerable<GameCoreJsonEntity> jsonEntities)
	{
		Root = root;
		JsonEntities = jsonEntities;
	}

	public IEnumerable<GameCore> GetGameCores()
	{
		List<GameCore> cores = new List<GameCore>();
		foreach (GameCoreJsonEntity jsonEntity in JsonEntities)
		{
			try
			{
				GameCore gameCore = new GameCore
				{
					Id = jsonEntity.Id,
					Type = jsonEntity.Type,
					MainClass = jsonEntity.MainClass,
					InheritsFrom = jsonEntity.InheritsFrom,
					JavaVersion = (jsonEntity.JavaVersion?.MajorVersion).Value,
					LibraryResources = new LibraryParser(jsonEntity.Libraries, Root).GetLibraries().ToList(),
					Root = Root
				};
				if (string.IsNullOrEmpty(jsonEntity.InheritsFrom) && jsonEntity.Downloads != null)
				{
					gameCore.ClientFile = GetClientFile(jsonEntity);
				}
				if (string.IsNullOrEmpty(jsonEntity.InheritsFrom) && jsonEntity.Logging != null && jsonEntity.Logging.Client != null)
				{
					gameCore.LogConfigFile = GetLogConfigFile(jsonEntity);
				}
				if (string.IsNullOrEmpty(jsonEntity.InheritsFrom) && jsonEntity.AssetIndex != null)
				{
					gameCore.AssetIndexFile = GetAssetIndexFile(jsonEntity);
				}
				if (jsonEntity.MinecraftArguments != null)
				{
					gameCore.BehindArguments = HandleMinecraftArguments(jsonEntity.MinecraftArguments);
				}
				if (jsonEntity.Arguments != null && jsonEntity.Arguments.Game != null)
				{
					IEnumerable<string> behindArguments;
					if (gameCore.BehindArguments != null)
					{
						behindArguments = gameCore.BehindArguments.Union(HandleArgumentsGame(jsonEntity.Arguments));
					}
					else
					{
						IEnumerable<string> enumerable = HandleArgumentsGame(jsonEntity.Arguments);
						behindArguments = enumerable;
					}
					gameCore.BehindArguments = behindArguments;
				}
				if (jsonEntity.Arguments != null && jsonEntity.Arguments.Jvm != null)
				{
					gameCore.FrontArguments = HandleArgumentsJvm(jsonEntity.Arguments);
				}
				else
				{
					gameCore.FrontArguments = new string[4] { "-Djava.library.path=${natives_directory}", "-Dminecraft.launcher.brand=${launcher_name}", "-Dminecraft.launcher.version=${launcher_version}", "-cp ${classpath}" };
				}
				cores.Add(gameCore);
			}
			catch (Exception item)
			{
				ErrorGameCores.Add((jsonEntity.Id, item));
			}
		}
		foreach (GameCore item2 in cores)
		{
			item2.Source = GetSource(item2);
			item2.HasModLoader = GetHasModLoader(item2);

			if (item2.HasModLoader) {
                item2.ModLoaderInfos = GetModLoaderInfos(item2);
            }

            if (string.IsNullOrEmpty(item2.InheritsFrom))
			{
				yield return item2;
				continue;
			}
			GameCore gameCore2 = null;
			foreach (GameCore item3 in cores)
			{
				if (item3.Id == item2.InheritsFrom)
				{
					gameCore2 = item3;
				}
			}
			if (gameCore2 != null)
			{
				yield return Combine(item2, gameCore2);
			}
		}
	}

	private FileResource GetClientFile(GameCoreJsonEntity entity)
	{
		string text = Path.Combine(Root.FullName, "versions", entity.Id, entity.Id + ".jar");
		return new FileResource
		{
			CheckSum = entity.Downloads["client"].Sha1,
			Size = entity.Downloads["client"].Size,
			Url = ((APIManager.Current != APIManager.Mojang) ? entity.Downloads["client"].Url.Replace("https://launcher.mojang.com", APIManager.Current.Host) : entity.Downloads["client"].Url),
			Root = Root,
			FileInfo = new FileInfo(text),
			Name = Path.GetFileName(text)
		};
	}

	private FileResource GetLogConfigFile(GameCoreJsonEntity entity)
	{
		string fileName = Path.Combine(Root.FullName, "versions", entity.Id, entity.Logging.Client.File.Id ??= Path.GetFileName(entity.Logging.Client.File.Url));
		return new FileResource
		{
			CheckSum = entity.Logging.Client.File.Sha1,
			Size = entity.Logging.Client.File.Size,
			Url = ((APIManager.Current != APIManager.Mojang) ? entity.Logging.Client.File.Url.Replace("https://launcher.mojang.com", APIManager.Current.Host) : entity.Logging.Client.File.Url),
			Name = entity.Logging.Client.File.Id,
			FileInfo = new FileInfo(fileName),
			Root = Root
		};
	}

	private FileResource GetAssetIndexFile(GameCoreJsonEntity entity)
	{
		string fileName = Path.Combine(Root.FullName, "assets", "indexes", entity.AssetIndex.Id + ".json");
		return new FileResource
		{
			CheckSum = entity.AssetIndex.Sha1,
			Size = entity.AssetIndex.Size,
			Url = ((APIManager.Current != APIManager.Mojang) ? entity.AssetIndex.Url.Replace("https://launchermeta.mojang.com", APIManager.Current.Host).Replace("https://piston-meta.mojang.com", APIManager.Current.Host) : entity.AssetIndex.Url),
			Name = entity.AssetIndex.Id + ".json",
			FileInfo = new FileInfo(fileName),
			Root = Root
		};
	}

	private string GetSource(GameCore core)
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (core.InheritsFrom != null)
			{
				return core.InheritsFrom;
			}
			string path = Path.Combine(core.Root.FullName, "versions", core.Id, core.Id + ".json");
			if (File.Exists(path))
			{
				JObject jObject = JObject.Parse(File.ReadAllText(path));
				if (jObject.ContainsKey("patches"))
				{
					return ((object)((JArray)jObject["patches"])[0][(object)"version"]).ToString();
				}
				if (jObject.ContainsKey("clientVersion"))
				{
					return ((object)jObject["clientVersion"]).ToString();
				}
			}
		}
		catch
		{
		}
		return core.Id;
	}

	private bool GetHasModLoader(GameCore core)
	{
		using (IEnumerator<string> enumerator = core.BehindArguments.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				switch (enumerator.Current)
				{
				case "--tweakClass optifine.OptiFineTweaker":
				case "--tweakClass net.minecraftforge.fml.common.launcher.FMLTweaker":
				case "--fml.forgeGroup net.minecraftforge":
					return true;
				}
			}
		}
		foreach (string frontArgument in core.FrontArguments)
		{
			if (frontArgument.Contains("-DFabricMcEmu= net.minecraft.client.main.Main"))
			{
				return true;
			}
		}
		switch (core.MainClass)
		{
		    case "net.minecraft.client.main.Main":
		    case "net.minecraft.launchwrapper.Launch":
		    case "com.mojang.rubydung.RubyDung":		
		    	return false;
            default:
		    	return true;
		}
	}

	private IEnumerable<ModLoaderInfo> GetModLoaderInfos(GameCore core)
	{
        var libFind = core.LibraryResources.Where(lib =>
        {
            var lowerName = lib.Name.ToLower();
			
			return lowerName.StartsWith("optifine:optifine") ||
			lowerName.StartsWith("net.minecraftforge:forge:") ||
			lowerName.StartsWith("net.minecraftforge:fmlloader:") ||
			lowerName.StartsWith("net.fabricmc:fabric-loader") ||
			lowerName.StartsWith("com.mumfrey:liteloader:") ||
			lowerName.StartsWith("org.quiltmc:quilt-loader");
        });

        foreach (var lib in libFind)
        {
            var lowerName = lib.Name.ToLower();
            var id = lib.Name.Split(':')[2];

            if (lowerName.StartsWith("optifine:optifine"))
                yield return new() { ModLoaderType = ModLoaderType.OptiFine, Version = id.Substring(id.IndexOf('_') + 1), };
            else if (lowerName.StartsWith("net.minecraftforge:forge:") ||
                lowerName.StartsWith("net.minecraftforge:fmlloader:"))
                yield return new() { ModLoaderType = ModLoaderType.Forge, Version = id.Split('-')[1] };
            else if (lowerName.StartsWith("net.fabricmc:fabric-loader"))
                yield return new() { ModLoaderType = ModLoaderType.Fabric, Version = id };
            else if (lowerName.StartsWith("com.mumfrey:liteloader:"))
                yield return new() { ModLoaderType = ModLoaderType.LiteLoader, Version = id };
            else if (lowerName.StartsWith("org.quiltmc:quilt-loader"))
                yield return new() { ModLoaderType = ModLoaderType.Quilt, Version = id };
        }
    }

	private IEnumerable<string> HandleMinecraftArguments(string minecraftArguments)
	{
		return ArgumnetsGroup(minecraftArguments.Replace("  ", " ").Split(' '));
	}

	private IEnumerable<string> HandleArgumentsGame(ArgumentsJsonEntity entity)
	{
		return ArgumnetsGroup(from x in entity.Game
			where x is string
			select x.ToString().ToPath());
	}

	private IEnumerable<string> HandleArgumentsJvm(ArgumentsJsonEntity entity)
	{
		return ArgumnetsGroup(from x in entity.Jvm
			where x is string
			select x.ToString().ToPath());
	}

	private static IEnumerable<string> ArgumnetsGroup(IEnumerable<string> vs)
	{
		List<string> cache = new List<string>();
		foreach (string item in vs)
		{
			if (cache.Any() && cache[0].StartsWith("-") && item.StartsWith("-"))
			{
				yield return cache[0].Trim(' ');
				cache = new List<string> { item };
			}
			else if (vs.Last() == item && !cache.Any())
			{
				yield return item.Trim(' ');
			}
			else
			{
				cache.Add(item);
			}
			if (cache.Count == 2)
			{
				yield return string.Join(" ", cache).Trim(' ');
				cache = new List<string>();
			}
		}
	}

	private GameCore Combine(GameCore raw, GameCore inheritsFrom)
	{
		raw.AssetIndexFile = inheritsFrom.AssetIndexFile;
		raw.ClientFile = inheritsFrom.ClientFile;
		raw.LogConfigFile = inheritsFrom.LogConfigFile;
		raw.JavaVersion = inheritsFrom.JavaVersion;
		raw.Type = inheritsFrom.Type;
		raw.LibraryResources = raw.LibraryResources.Union(inheritsFrom.LibraryResources).ToList();
		raw.BehindArguments = inheritsFrom.BehindArguments.Union(raw.BehindArguments).ToList();
		raw.FrontArguments = raw.FrontArguments.Union(inheritsFrom.FrontArguments).ToList();
		return raw;
	}
}
