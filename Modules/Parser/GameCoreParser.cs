using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using MinecraftLaunch.Modules.Enum;
using MinecraftLaunch.Modules.Models.Download;
using MinecraftLaunch.Modules.Models.Launch;
using MinecraftLaunch.Modules.Utilities;


namespace MinecraftLaunch.Modules.Parser;

public class GameCoreParser {
    public DirectoryInfo Root { get; set; }

    public IEnumerable<GameCoreJsonEntity> JsonEntities { get; set; }

    public List<(string, Exception)> ErrorGameCores { get; private set; } = new List<(string, Exception)>();


    public GameCoreParser(DirectoryInfo root, IEnumerable<GameCoreJsonEntity> jsonEntities) {
        Root = root;
        JsonEntities = jsonEntities;
    }

    public IEnumerable<GameCore> GetGameCores() {
        List<GameCore> gameCores = new();
        foreach (GameCoreJsonEntity jsonEntity in JsonEntities) {
            try {
                GameCore gameCore = new GameCore {
                    Id = jsonEntity.Id,
                    Type = jsonEntity.Type,
                    MainClass = jsonEntity.MainClass,
                    InheritsFrom = jsonEntity.InheritsFrom,
                    JavaVersion = (jsonEntity.JavaVersion?.MajorVersion).Value,
                    LibraryResources = new LibraryParser(jsonEntity.Libraries, Root).GetLibraries().ToList(),
                    Root = Root
                };

                if (string.IsNullOrEmpty(jsonEntity.InheritsFrom)) {
                    gameCore.ClientFile = GetClientFile(jsonEntity);
                    if (jsonEntity.Logging != null && jsonEntity.Logging.Client != null) {
                        gameCore.LogConfigFile = GetLogConfigFile(jsonEntity);
                    }
                    if (jsonEntity.AssetIndex != null) {
                        gameCore.AssetIndexFile = GetAssetIndexFile(jsonEntity);
                    }
                }

                if (jsonEntity.MinecraftArguments != null) {
                    gameCore.BehindArguments = HandleMinecraftArguments(jsonEntity.MinecraftArguments);
                }

                if (jsonEntity.Arguments != null && jsonEntity.Arguments.Game != null) {
                    IEnumerable<string> behindArguments;
                    if (gameCore.BehindArguments != null) {
                        behindArguments = gameCore.BehindArguments.Union(HandleArgumentsGame(jsonEntity.Arguments));
                    } else {
                        IEnumerable<string> enumerable = HandleArgumentsGame(jsonEntity.Arguments);
                        behindArguments = enumerable;
                    }
                    gameCore.BehindArguments = behindArguments;
                }

                if (jsonEntity.Arguments != null && jsonEntity.Arguments.Jvm != null) {
                    gameCore.FrontArguments = HandleArgumentsJvm(jsonEntity.Arguments);
                } else {
                    gameCore.FrontArguments = new string[4] { "-Djava.library.path=${natives_directory}", "-Dminecraft.launcher.brand=${launcher_name}", "-Dminecraft.launcher.version=${launcher_version}", "-cp ${classpath}" };
                }

                gameCores.Add(gameCore);
            }
            catch (Exception exception) {
                ErrorGameCores.Add((jsonEntity.Id, exception));
            }
        }

        Dictionary<string, GameCore> gameCoresDict = gameCores?.ToDictionary(core => core.Id!)!;
        foreach (GameCore gameCore in gameCores!) {
            gameCore.Source = GetSource(gameCore);
            gameCore.HasModLoader = GetHasModLoader(gameCore);

            if (gameCore.HasModLoader) {
                gameCore.ModLoaderInfos = GetModLoaderInfos(gameCore);
            }

            if (!string.IsNullOrEmpty(gameCore.InheritsFrom)) {
                if (gameCoresDict.TryGetValue(gameCore.InheritsFrom, out var inheritedGameCore)) {
                    yield return Combine(gameCore, inheritedGameCore);
                }
            } else {
                yield return gameCore;
            }
        }
    }

    private FileResource GetClientFile(GameCoreJsonEntity entity) {
        string text = Path.Combine(Root.FullName, "versions", entity.Id, entity.Id + ".jar");
        return new FileResource {
            CheckSum = entity.Downloads["client"].Sha1,
            Size = entity.Downloads["client"].Size,
            Url = ((APIManager.Current != APIManager.Mojang) ? entity.Downloads["client"].Url.Replace("https://launcher.mojang.com", APIManager.Current.Host) : entity.Downloads["client"].Url),
            Root = Root,
            FileInfo = new FileInfo(text),
            Name = Path.GetFileName(text)
        };
    }

    private FileResource GetLogConfigFile(GameCoreJsonEntity entity) {
        string fileName = Path.Combine(Root.FullName, "versions", entity.Id, entity.Logging.Client.File.Id ??= Path.GetFileName(entity.Logging.Client.File.Url));
        return new FileResource {
            CheckSum = entity.Logging.Client.File.Sha1,
            Size = entity.Logging.Client.File.Size,
            Url = ((APIManager.Current != APIManager.Mojang) ? entity.Logging.Client.File.Url.Replace("https://launcher.mojang.com", APIManager.Current.Host) : entity.Logging.Client.File.Url),
            Name = entity.Logging.Client.File.Id,
            FileInfo = new FileInfo(fileName),
            Root = Root
        };
    }

    private FileResource GetAssetIndexFile(GameCoreJsonEntity entity) {
        string fileName = Path.Combine(Root.FullName, "assets", "indexes", entity.AssetIndex.Id + ".json");
        return new FileResource {
            CheckSum = entity.AssetIndex.Sha1,
            Size = entity.AssetIndex.Size,
            Url = ((APIManager.Current != APIManager.Mojang) ? entity.AssetIndex.Url.Replace("https://launchermeta.mojang.com", APIManager.Current.Host).Replace("https://piston-meta.mojang.com", APIManager.Current.Host) : entity.AssetIndex.Url),
            Name = entity.AssetIndex.Id + ".json",
            FileInfo = new FileInfo(fileName),
            Root = Root
        };
    }

    private string GetSource(GameCore core) {
        try {
            if (core.InheritsFrom != null) {
                return core.InheritsFrom;
            }
            string path = Path.Combine(core.Root.FullName, "versions", core.Id, core.Id + ".json");
            if (File.Exists(path)) {
                using var document = JsonDocument.Parse(File.ReadAllText(path));
                if (document.RootElement.TryGetProperty("patches", out var patchesElement)) {
                    return patchesElement.EnumerateArray().First().GetProperty("version").GetString()!;
                }
                if (document.RootElement.TryGetProperty("clientVersion", out var clientVersionElement)) {
                    return clientVersionElement.GetString()!;
                }
            }
        }
        catch {
        }
        return core.Id;
    }

    private bool GetHasModLoader(GameCore core) {
        using (IEnumerator<string> enumerator = core.BehindArguments.GetEnumerator()) {
            while (enumerator.MoveNext()) {
                switch (enumerator.Current) {
                    case "--tweakClass optifine.OptiFineTweaker":
                    case "--tweakClass net.minecraftforge.fml.common.launcher.FMLTweaker":
                    case "--fml.forgeGroup net.minecraftforge":
                        return true;
                }
            }
        }
        foreach (string frontArgument in core.FrontArguments) {
            if (frontArgument.Contains("-DFabricMcEmu= net.minecraft.client.main.Main")) {
                return true;
            }
        }
        switch (core.MainClass) {
            case "net.minecraft.client.main.Main":
            case "net.minecraft.launchwrapper.Launch":
            case "com.mojang.rubydung.RubyDung":
                return false;
            default:
                return true;
        }
    }

    private IEnumerable<ModLoaderInfo> GetModLoaderInfos(GameCore core) {
        var libFind = core.LibraryResources.Where(lib => {
            var lowerName = lib.Name.ToLower();

            return lowerName.StartsWith("optifine:optifine") ||
            lowerName.StartsWith("net.minecraftforge:forge:") ||
            lowerName.StartsWith("net.minecraftforge:fmlloader:") ||
            lowerName.StartsWith("net.fabricmc:fabric-loader") ||
            lowerName.StartsWith("com.mumfrey:liteloader:") ||
            lowerName.StartsWith("org.quiltmc:quilt-loader:") ||
            lowerName.StartsWith("net.neoforged.fancymodloader");
        });
       
        foreach (var lib in libFind) {
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
            else if (lowerName.StartsWith("net.neoforged.fancymodloader"))
                yield return new() { ModLoaderType = ModLoaderType.NeoForged, Version = id };
        }
    }

    private IEnumerable<string> HandleMinecraftArguments(string minecraftArguments) => GroupArguments(minecraftArguments.Replace("  ", " ").Split(' '));

    private IEnumerable<string> HandleArgumentsGame(ArgumentsJsonEntity entity) => GroupArguments(entity.Game.Where(x => x.ValueKind == JsonValueKind.String).Select(x => x.GetString()!.ToPath()));

    private IEnumerable<string> HandleArgumentsJvm(ArgumentsJsonEntity entity) => GroupArguments(entity.Jvm.Where(x => x.ValueKind == JsonValueKind.String).Select(x => x.GetString()!.ToPath()));

    private static IEnumerable<string> GroupArguments(IEnumerable<string> arguments) {
        List<string> cache = new List<string>();
        string lastArgument = arguments.Last();

        foreach (string argument in arguments) {
            if (cache.Any() && cache[0].StartsWith("-") && argument.StartsWith("-")) {
                yield return cache[0].Trim();
                cache.Clear();
                cache.Add(argument);
            } else if (lastArgument == argument && !cache.Any()) {
                yield return argument.Trim();
            } else {
                cache.Add(argument);
            }
            if (cache.Count == 2) {
                yield return string.Join(" ", cache).Trim();
                cache.Clear();
            }
        }
    }

    private GameCore Combine(GameCore raw, GameCore inheritsFrom) {
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
