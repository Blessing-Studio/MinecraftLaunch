using MinecraftLaunch.Modules.ArgumentsBuilders;
using MinecraftLaunch.Modules.Models.Launch;
using MinecraftLaunch.Modules.Parser;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace MinecraftLaunch.Modules.Utilities;

public class GameCoreUtil {
    public DirectoryInfo? Root { get; private set; }

    public List<(string, Exception)>? ErrorGameCores { get; private set; }

    public GameCoreUtil() {
        Root = new DirectoryInfo(".minecraft");
    }

    public GameCoreUtil(string path) {
        Root = new DirectoryInfo(path);
    }

    public GameCoreUtil(DirectoryInfo root) {
        Root = root;
    }

    public void Delete(string id) {
        DirectoryInfo directory = new DirectoryInfo(Path.Combine(Root.FullName, "versions", id));
        if (directory.Exists) {
            directory.DeleteAllFiles();
        }

        directory.Delete();
    }

    public GameCore GetGameCore(string gameId) {
        return GetGameCores().FirstOrDefault(core => core.Id == gameId)!;
    }

    public IEnumerable<GameCore> GetGameCores() {
        var versionsFolder = new DirectoryInfo(Path.Combine(Root.FullName, "versions"));
        if (!versionsFolder.Exists) {
            versionsFolder.Create();
            return Array.Empty<GameCore>();
        }

        var entities = GetEntitiesFromDirectories(versionsFolder.GetDirectories());
        var parser = new GameCoreParser(Root, entities);
        ErrorGameCores = parser.ErrorGameCores;

        return parser.GetGameCores();
    }

    public GameCore RenameGameCore(string oldid, string newid) {
        string versionsPath = Path.Combine(Root!.FullName, "versions");
        string oldPath = Path.Combine(versionsPath, oldid);
        string newPath = Path.Combine(versionsPath, newid);

        try {
            var entity = JsonSerializer.Deserialize<GameCoreJsonEntity>(File.ReadAllText(Path.Combine(oldPath, $"{oldid}.json")));
            entity!.Id = newid;

            foreach (var gameCore in GetGameCores(Root.FullName).Where(gameCore => gameCore.InheritsFrom == oldid)) {
                gameCore.InheritsFrom = newid;
                File.WriteAllText(Path.Combine(gameCore.Root!.FullName, "versions", gameCore.Id!, $"{gameCore.Id}.json"), 
                    JsonSerializer.Serialize(gameCore));
            }

            File.WriteAllText(Path.Combine(oldPath, $"{oldid}.json"), entity.ToJson());

            if (File.Exists(Path.Combine(oldPath, $"{newid}.jar"))) {
                File.Move(Path.Combine(oldPath, $"{oldid}.jar"), Path.Combine(oldPath, $"{newid}.jar"));
            }

            File.Move(Path.Combine(oldPath, $"{oldid}.json"), Path.Combine(oldPath, $"{newid}.json"));
            Directory.Move(oldPath, newPath);
        }
        catch (Exception ex) {
            Trace.WriteLine($"[MinecraftLaunch][GameCoreToolkit/ReName]: {ex.Message}\n {ex.StackTrace}");
            throw;
        }

        return GetGameCore(newid);
    }

    public IEnumerable<GameCore> SearchGameCores(string searchText) {
        var gameCores = GetGameCores();
        var filteredCores = new List<GameCore>();
        var lowerSearchText = searchText.ToLower();

        var initialSearch = gameCores.Where(x => x.Id!.ToLower().Contains(lowerSearchText));

        if (!initialSearch.Any()) {
            filteredCores.AddRange(PolymerizeSearch(lowerSearchText, gameCores) ?? new List<GameCore>());
        } else {
            filteredCores.AddRange(initialSearch);
        }

        if (!filteredCores.Any()) {
            filteredCores.AddRange(FullSpellSearch(lowerSearchText, gameCores));
            filteredCores.AddRange(InitialSpellSearch(lowerSearchText, gameCores, filteredCores));
        }

        GC.Collect();
        return filteredCores.Distinct();
    }

    public static GameCore RenameGameCore(string rootPath, string oldId, string newId) {
        string versionsPath = Path.Combine(rootPath, "versions");
        FileInfo gameJsonFile = new FileInfo(Path.Combine(versionsPath, oldId, $"{oldId}.json")),
                 gameJarFile = new FileInfo(Path.Combine(versionsPath, oldId, $"{oldId}.jar"));
        DirectoryInfo gameFolder = new DirectoryInfo(Path.Combine(rootPath, "versions", oldId));
        GameCoreJsonEntity gameEntity = new GameCoreJsonEntity();

        try {
            gameEntity = gameEntity.ToJsonEntity(File.ReadAllText(gameJsonFile.FullName));
            gameEntity.Id = newId;
            foreach (GameCore gameCore in GetGameCores(rootPath)) {
                if (gameCore.InheritsFrom == oldId) {
                    gameCore.InheritsFrom = newId;
                    File.WriteAllText(Path.Combine(gameCore.Root?.FullName, "versions", gameCore.Id, $"{gameCore.Id}.json"),
                        GetGameCoreJsonEntity(rootPath, gameCore.Id, gameCore.InheritsFrom)
                        .ToJson());
                }
            }
            File.WriteAllText(gameJsonFile.FullName, gameEntity.ToJson());

            if (File.Exists(Path.Combine(versionsPath, oldId, $"{newId}.jar"))) {
                File.Move(gameJarFile.FullName, Path.Combine(versionsPath, oldId, $"{newId}.jar"));
            }

            File.Move(gameJsonFile.FullName, Path.Combine(versionsPath, oldId, $"{newId}.json"));
            Directory.Move(gameFolder.FullName, Path.Combine(versionsPath, newId));
        }
        catch (Exception ex) {
            Trace.WriteLine($"[MinecraftLaunch][GameCoreToolkit/RenameGameCore]: {ex.Message}\n {ex.StackTrace}");
            throw;
        }

        return GetGameCore(rootPath, newId);
    }

    public static GameCore GetGameCore(string rootPath, string gameId) {
        var gameCores = string.IsNullOrEmpty(rootPath) ? GetGameCores(".minecraft") 
            : GetGameCores(rootPath);

        return gameCores.FirstOrDefault(core => core.Id == gameId)!;
    }

    public static void Delete(string root, string Id) {
        DirectoryInfo directory = new DirectoryInfo(Path.Combine(root, "versions", Id));
        if (directory.Exists) {
            directory.DeleteAllFiles();
        }

        directory.Delete();
    }

    public static IEnumerable<GameCore> GetGameCores(string rootPath) {
        List<GameCoreJsonEntity> gameEntities = new();
        DirectoryInfo versionsFolder = new(Path.Combine(rootPath, "versions"));
        if (!versionsFolder.Exists) {
            versionsFolder.Create();
            return Array.Empty<GameCore>();
        }

        var directories = versionsFolder.EnumerateDirectories();
        foreach (DirectoryInfo directory in directories) {
            var files = directory.EnumerateFiles();
            foreach (FileInfo file in files) {
                if (file.Name == $"{directory.Name}.json") {
                    GameCoreJsonEntity gameEntity = new();
                    gameEntity = gameEntity.ToJsonEntity(File.ReadAllText(file.FullName));
                    gameEntities.Add(gameEntity);
                }
            }
        }

        return new GameCoreParser(new DirectoryInfo(rootPath), gameEntities)
            .GetGameCores();
    }

    public static async ValueTask<bool> CreateLaunchScriptAsync(GameCore gameCore, LaunchConfig launchConfig, string scriptPath) {
        try {
            string javaPath = string.Empty;
            StringBuilder scriptBuilder = new();
            JavaMinecraftArgumentsBuilder argumentsBuilder = new(gameCore, launchConfig);
            if (EnvironmentUtil.IsWindow) {
                javaPath = Path.Combine(launchConfig.JvmConfig.JavaPath.Directory!.FullName, "java.exe");
                scriptBuilder.AppendLine($"@echo off")
                             .AppendLine($"title Launch - {gameCore.Id}")
                             .AppendLine($"set APPDATA={gameCore.Root.Parent.FullName}")
                             .AppendLine($"set INST_NAME={gameCore.Id}")
                             .AppendLine($"set INST_ID={gameCore.Id}")
                             .AppendLine($"set INST_DIR={gameCore.GetGameCorePath(launchConfig.IsEnableIndependencyCore)}")
                             .AppendLine($"set INST_MC_DIR={gameCore.Root.FullName}")
                             .AppendLine($"set INST_JAVA=\"{javaPath}\"")
                             .AppendLine($"cd /D {gameCore.Root.FullName}")
                             .AppendLine($"\"{javaPath}\" {string.Join(' ', argumentsBuilder.Build())}")
                             .AppendLine($"pause");
            } else if (EnvironmentUtil.IsMac) {
                scriptBuilder.AppendLine($"export INST_NAME={gameCore.Id}")
                             .AppendLine($"export INST_ID={gameCore.Id}")
                             .AppendLine($"export INST_DIR=\"{gameCore.GetGameCorePath(launchConfig.IsEnableIndependencyCore)}\"")
                             .AppendLine($"export INST_MC_DIR=\"{gameCore.Root.FullName}\"")
                             .AppendLine($"export INST_JAVA=\"{javaPath}\"")
                             .AppendLine($"cd \"{gameCore.Root!.FullName}\"")
                             .AppendLine($"\"{javaPath}\" {string.Join(' ', argumentsBuilder.Build())}");
            } else if (EnvironmentUtil.IsLinux) {
                scriptBuilder.AppendLine($"export INST_JAVA={javaPath}")
                             .AppendLine($"export INST_MC_DIR={gameCore.Root!.FullName}")
                             .AppendLine($"export INST_NAME={gameCore.Id}")
                             .AppendLine($"export INST_ID={gameCore.Id}")
                             .AppendLine($"export INST_DIR={gameCore.GetGameCorePath(launchConfig.IsEnableIndependencyCore)}")
                             .AppendLine($"cd {gameCore.Root!.FullName}")
                             .AppendLine($"{javaPath} {string.Join(' ', argumentsBuilder.Build())}");
            }

            await File.WriteAllTextAsync(scriptPath, scriptBuilder.ToString());
            return true;
        }
        catch (Exception) {
            return false;
        }
    }

    public static GameCoreJsonEntity GetGameCoreJsonEntity(string root, string id, string inheritsfrom) {
        DirectoryInfo versionsFolder = new DirectoryInfo(Path.Combine(root, "versions"));
        if (!versionsFolder.Exists) {
            versionsFolder.Create();
            return null;
        }

        DirectoryInfo[] directories = versionsFolder.GetDirectories();
        foreach (DirectoryInfo item in directories) {
            FileInfo[] files2 = item.GetFiles();
            foreach (FileInfo files in files2) {
                if (files.Name == item.Name + ".json") {
                    GameCoreJsonEntity entity = new GameCoreJsonEntity();
                    entity = entity.ToJsonEntity(File.ReadAllText(files.FullName));
                    if (entity.Id == id) {
                        entity.InheritsFrom = inheritsfrom;
                        return entity;
                    }
                }
            }
        }

        return null!;
    }

    private IEnumerable<GameCore> FullSpellSearch(string lowerText, IEnumerable<GameCore> gameCores) {
        return gameCores.Where(x => StringUtil.GetSpell(x.Id!).ToLower().Contains(lowerText));
    }

    private IEnumerable<GameCore> InitialSpellSearch(string lowerText, IEnumerable<GameCore> gameCores, List<GameCore> endCores) {
        return gameCores.Where(x => {
            var firstSpell = StringUtil.GetFirstSpell(x.Id!).ToLower();

            if (firstSpell.Contains(lowerText)) {
                return !endCores.Any(c => c.Id!.ToLower() == firstSpell);
            }

            return false;
        });
    }

    private IEnumerable<GameCore> PolymerizeSearch(string searchText, IEnumerable<GameCore> gameCores) {
        var filteredCores = new List<GameCore>();
        var lowerSearchText = searchText.ToLower();

        if (lowerSearchText.StartsWith("-v")) {
            var condition = lowerSearchText.Replace("-v", "")
                .TrimStart();

            filteredCores.AddRange(SearchByVersion(condition, gameCores));
        }

        if (lowerSearchText.StartsWith("-l")) {
            var condition = lowerSearchText.Replace("-l", "")
                .TrimStart();

            filteredCores.AddRange(SearchByModLoader(condition, gameCores));
        }

        return filteredCores;

        IEnumerable<GameCore> SearchByVersion(string condition, IEnumerable<GameCore> gameCores) {
            return gameCores.Where(x => !string.IsNullOrEmpty(x.InheritsFrom) && x.InheritsFrom.Contains(condition)
                                        || !string.IsNullOrEmpty(x.Source) && x.Source.Contains(condition));
        }

        IEnumerable<GameCore> SearchByModLoader(string condition, IEnumerable<GameCore> gameCores) {
            return gameCores.Where(x => x.ModLoaderInfos.Any(i => i.ModLoaderType.ToString().ToLower() == condition));
        }
    }

    private List<GameCoreJsonEntity> GetEntitiesFromDirectories(DirectoryInfo[] directories) {
        var entities = new List<GameCoreJsonEntity>();

        foreach (var directory in directories.AsParallel()) {
            var jsonFile = directory.GetFiles().FirstOrDefault(file => file.Name == $"{directory.Name}.json");

            if (jsonFile != null) {
                try {
                    var entity = new GameCoreJsonEntity().ToJsonEntity(File.ReadAllText(jsonFile.FullName));
                    entities.Add(entity);
                }
                catch { }
            }
        }

        return entities;
    }

    public static implicit operator GameCoreUtil(string path) => new GameCoreUtil(path);

    public static implicit operator GameCoreUtil(DirectoryInfo path) => new GameCoreUtil(path);

    public static implicit operator GameCoreUtil(GameCore path) => new GameCoreUtil(path.Root!);
}
