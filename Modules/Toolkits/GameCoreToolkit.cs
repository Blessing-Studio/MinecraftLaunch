using MinecraftLaunch.Modules.Models.Launch;
using MinecraftLaunch.Modules.Parser;

namespace MinecraftLaunch.Modules.Toolkits;

public class GameCoreToolkit
{
	public DirectoryInfo Root { get; private set; }

	public List<(string, Exception)> ErrorGameCores { get; private set; }

	public GameCore GameCoreNameChange(string oldid, string newid)
	{
		FileInfo gamejson = new FileInfo(Path.Combine(Root.FullName, "versions", oldid, oldid + ".json"));
		FileInfo gameJar = new FileInfo(Path.Combine(Root.FullName, "versions", oldid, oldid + ".jar"));
		DirectoryInfo gameFolder = new DirectoryInfo(Path.Combine(Root.FullName, "versions", oldid));
		GameCoreJsonEntity entity = new GameCoreJsonEntity();
		try
		{
			entity = entity.ToJsonEntity(File.ReadAllText(gamejson.FullName));
			entity.Id = newid;
			foreach (GameCore i in GetGameCores(Root.FullName).ToList())
			{
				if (i.InheritsFrom == oldid)
				{
					i.InheritsFrom = newid;
					File.WriteAllText(Path.Combine(i.Root?.FullName, "versions", i.Id, i.Id + ".json"), GetGameCoreJsonEntity(Root.FullName, i.Id, i.InheritsFrom).ToJson());
				}
			}
			File.WriteAllText(gamejson.FullName, entity.ToJson());
			File.Move(gameJar.FullName, Path.Combine(Root.FullName, "versions", oldid, newid + ".jar"));
			File.Move(gamejson.FullName, Path.Combine(Root.FullName, "versions", oldid, newid + ".json"));
			Directory.Move(gameFolder.FullName, Path.Combine(Root.FullName, "versions", newid));
		}
		catch
		{
		}
		return GetGameCore(newid);
	}

	public GameCore GetGameCore(string id)
	{
		foreach (GameCore core in GetGameCores())
		{
			if (core.Id == id)
			{
				return core;
			}
		}
		return null;
	}

	public void Delete(string Id)
	{
		DirectoryInfo directory = new DirectoryInfo(Path.Combine(Root.FullName, "versions", Id));
		if (directory.Exists)
		{
			directory.DeleteAllFiles();
		}
		directory.Delete();
	}

	public IEnumerable<GameCore> GetGameCores() {
	
		List<GameCoreJsonEntity> entities = new List<GameCoreJsonEntity>();
		DirectoryInfo versionsFolder = new DirectoryInfo(Path.Combine(Root.FullName, "versions"));
		if (!versionsFolder.Exists)
		{
			versionsFolder.Create();
			return Array.Empty<GameCore>();
		}
		DirectoryInfo[] directories = versionsFolder.GetDirectories();
		foreach (DirectoryInfo item in directories.AsParallel())
		{
			FileInfo[] files2 = item.GetFiles();
			foreach (FileInfo files in files2.AsParallel())
			{
				if (files.Name == item.Name + ".json")
				{
					GameCoreJsonEntity entity = new GameCoreJsonEntity();
					try
					{
						entity = entity.ToJsonEntity(File.ReadAllText(files.FullName));
						entities.Add(entity);
					}
					catch
					{}					
				}
			}
		}
		GameCoreParser parser = new GameCoreParser(Root, entities);
		var gameCores = parser.GetGameCores().ToArray();
		ErrorGameCores = parser.ErrorGameCores;
		return gameCores;
	}

	public IEnumerable<GameCore> GameCoreScearh(string text) { 
		var gameCores = GetGameCores();
		var endCores = new List<GameCore>();

		var firstScearh = gameCores.Where(x => x.Id!.ToLower().Contains(text.ToLower()));//标准筛查 -1

		if (!firstScearh.Any()) {			
			endCores.AddRange(PolymerizeScearh(text, gameCores) ?? new List<GameCore>());//条件筛查 -2
		} else endCores.AddRange(firstScearh);

		if (!endCores.Any()) {//拼音筛查 -3 End	
			endCores.AddRange(gameCores.Where(x =>//全拼筛查
			{			
				try {
					var spell = StringToolkit.GetSpell(x.Id!).ToLower();

                    if (spell.Contains(text.ToLower())) {
						return true;
					}
				}
				catch (Exception) {
					throw;
				}

				return false;
			}));

			endCores.AddRange(gameCores.Where(x =>//首字母筛查
			{
				try {
                    var firstspell = StringToolkit.GetFirstSpell(x.Id!).ToLower();

                    if (firstspell.Contains(text.ToLower())) {
						foreach(var c in endCores) { //mlgb，与标准筛查冲突了，手动检测是否有相同的游戏核心
							if (c.Id!.ToLower() == firstspell) {
								return false;
							}
						}

                        return true;
                    }
                }
                catch (Exception) {
					throw;
				}

				return false;
			}));
		}

		GC.Collect();
		return endCores.Distinct();
	}

	public static GameCore GameCoreNameChange(string root, string oldid, string newid)
	{
		FileInfo gamejson = new FileInfo(Path.Combine(root, "versions", oldid, oldid + ".json"));
		FileInfo gameJar = new FileInfo(Path.Combine(root, "versions", oldid, oldid + ".jar"));
		DirectoryInfo gameFolder = new DirectoryInfo(Path.Combine(root, "versions", oldid));
		GameCoreJsonEntity entity = new GameCoreJsonEntity();
		try
		{
			entity = entity.ToJsonEntity(File.ReadAllText(gamejson.FullName));
			entity.Id = newid;
			foreach (GameCore i in GetGameCores(root).ToList())
			{
				if (i.InheritsFrom == oldid)
				{
					i.InheritsFrom = newid;
					File.WriteAllText(Path.Combine(i.Root?.FullName, "versions", i.Id, i.Id + ".json"), GetGameCoreJsonEntity(root, i.Id, i.InheritsFrom).ToJson());
				}
			}
			File.WriteAllText(gamejson.FullName, entity.ToJson());
			File.Move(gameJar.FullName, Path.Combine(root, "versions", oldid, newid + ".jar"));
			File.Move(gamejson.FullName, Path.Combine(root, "versions", oldid, newid + ".json"));
			Directory.Move(gameFolder.FullName, Path.Combine(root, "versions", newid));
		}
		catch
		{
		}
		return GetGameCore(root, newid);
	}

	public static GameCore GetGameCore(string root, string id)
	{
		if (string.IsNullOrEmpty(root))
		{
			foreach (GameCore core2 in GetGameCores(".minecraft"))
			{
				if (core2.Id == id)
				{
					return core2;
				}
			}
		}
		foreach (GameCore core in GetGameCores(root))
		{
			if (core.Id == id)
			{
				return core;
			}
		}
		return null;
	}

	public static void Delete(string root, string Id)
	{
		DirectoryInfo directory = new DirectoryInfo(Path.Combine(root, "versions", Id));
		if (directory.Exists)
		{
			directory.DeleteAllFiles();
		}
		directory.Delete();
	}

	public static IEnumerable<GameCore> GetGameCores(string root)
	{
		List<GameCoreJsonEntity> entities = new List<GameCoreJsonEntity>();
		DirectoryInfo versionsFolder = new DirectoryInfo(Path.Combine(root, "versions"));
		if (!versionsFolder.Exists)
		{
			versionsFolder.Create();
			return Array.Empty<GameCore>();
		}
		DirectoryInfo[] directories = versionsFolder.GetDirectories();
		foreach (DirectoryInfo item in directories)
		{
			FileInfo[] files2 = item.GetFiles();
			foreach (FileInfo files in files2)
			{
				if (files.Name == item.Name + ".json")
				{
					GameCoreJsonEntity entity = new GameCoreJsonEntity();
					try
					{
						entity = entity.ToJsonEntity(File.ReadAllText(files.FullName));
						entities.Add(entity);
					}
					catch
					{
					}
				}
			}
		}
		return new GameCoreParser(new DirectoryInfo(root), entities).GetGameCores();
	}

	internal IEnumerable<GameCore> PolymerizeScearh(string text, IEnumerable<GameCore> cores) {
        var endCores = new List<GameCore>();

        if (text.StartsWith("-v")) {//通过版本搜索		
			var condition = text.Replace("-v", "").TrimStart().ToLower();
			
            endCores.AddRange(cores.Where(x =>
			{
                if (!string.IsNullOrEmpty(x.InheritsFrom) && x.InheritsFrom.Contains(condition)) {
					return true;
				}

				if (!string.IsNullOrEmpty(x.Source) && x.Source.Contains(condition)) {
					return true;
				}

				return false;
			}));
		}

		if (text.StartsWith("-l")) {//通过模组加载器搜索
            var condition = text.Replace("-l", "").TrimStart().ToLower();
			endCores.AddRange(cores.Where(x =>
			{
				foreach (var i in x.ModLoaderInfos) {
                    if (i.ModLoaderType.ToString().ToLower().Contains(condition)) {                 
						return true;
                    }
                }

                return false;
			}));
        }

		return endCores!;
	}

	internal static GameCoreJsonEntity GetGameCoreJsonEntity(string root, string id, string inheritsfrom)
	{
		new List<GameCoreJsonEntity>();
		DirectoryInfo versionsFolder = new DirectoryInfo(Path.Combine(root, "versions"));
		if (!versionsFolder.Exists)
		{
			versionsFolder.Create();
			return null;
		}
		DirectoryInfo[] directories = versionsFolder.GetDirectories();
		foreach (DirectoryInfo item in directories)
		{
			FileInfo[] files2 = item.GetFiles();
			foreach (FileInfo files in files2)
			{
				if (files.Name == item.Name + ".json")
				{
					GameCoreJsonEntity entity = new GameCoreJsonEntity();
					entity = entity.ToJsonEntity(File.ReadAllText(files.FullName));
					if (entity.Id == id)
					{
						entity.InheritsFrom = inheritsfrom;
						return entity;
					}
				}
			}
		}
		return null;
	}

	public GameCoreToolkit()
	{
		Root = new DirectoryInfo(".minecraft");
	}

	public GameCoreToolkit(string path)
	{
		Root = new DirectoryInfo(path);
	}

	public GameCoreToolkit(DirectoryInfo root)
	{
		Root = root;
	}

	public static implicit operator GameCoreToolkit(string path) => new GameCoreToolkit(path);

    public static implicit operator GameCoreToolkit(DirectoryInfo path) => new GameCoreToolkit(path);

    public static implicit operator GameCoreToolkit(GameCore path) => new GameCoreToolkit(path.Root!);
}
