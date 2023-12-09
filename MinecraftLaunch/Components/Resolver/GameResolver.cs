using System.Text.Json;
using System.Text.Json.Nodes;
using MinecraftLaunch.Classes.Interfaces;
using MinecraftLaunch.Classes.Models.Game;
using MinecraftLaunch.Classes.Models.Launch;
using MinecraftLaunch.Classes.Models.Exceptions;
using MinecraftLaunch.Extensions;
using MinecraftLaunch.Utilities;

namespace MinecraftLaunch.Components.Resolver {
    /// <summary>
    /// Minecraft 核心解析器
    /// </summary>
    public class GameResolver(string root) : IGameResolver, IResolver<GameJsonEntry> {
        public DirectoryInfo Root => new(root);

        /// <summary>
        /// 获取特定游戏实体信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public GameEntry GetGameEntity(string id) {
            var entity = Resolve(id);
            if (entity is null) {
                return null!;
            }

            var gameEntity = new GameEntry {
                Id = entity.Id,
                Type = entity.Type,
                GameFolderPath = root,
                IsInheritedFrom = false,
                MainClass = entity.MainClass,
                MainLoaderType = entity.GetGameLoaderType()
            };

            var assetsIndexFile = Path.Combine(root, "assets", "indexes", $"{entity.AssetIndex?.Id}.json");
            var jarFile = Path.Combine(gameEntity.OfVersionDirectoryPath(),
                $"{id}.jar");

            gameEntity.JarPath = jarFile;
            gameEntity.AssetsIndexJsonPath = assetsIndexFile;
            if (!string.IsNullOrEmpty(entity.InheritsFrom)) {
                var inheritsFrom = GetGameEntity(entity.InheritsFrom);
                if (inheritsFrom == null) {
                    return null;
                }

                gameEntity.IsInheritedFrom = true;
                gameEntity.InheritsFrom = inheritsFrom;
                gameEntity.JarPath ??= inheritsFrom.JarPath;
                gameEntity.AssetsIndexJsonPath = inheritsFrom.AssetsIndexJsonPath;
            }

            if (entity.MinecraftArguments != null) {
                gameEntity.BehindArguments = HandleMinecraftArguments(entity.MinecraftArguments);
            }

            if (entity.Arguments != null && entity.Arguments.Game != null) {
                IEnumerable<string> behindArguments;
                if (gameEntity.BehindArguments != null) {
                    behindArguments = gameEntity.BehindArguments.Union(HandleGameArguments(entity.Arguments));
                } else {
                    IEnumerable<string> enumerable = HandleGameArguments(entity.Arguments);
                    behindArguments = enumerable;
                }
                gameEntity.BehindArguments = behindArguments;
            }

            if (entity.Arguments != null && entity.Arguments.Jvm != null) {
                gameEntity.FrontArguments = HandleJvmArguments(entity.Arguments);
            } else {
                gameEntity.FrontArguments = ["-Djava.library.path=${natives_directory}",
                    "-Dminecraft.launcher.brand=${launcher_name}",
                    "-Dminecraft.launcher.version=${launcher_version}",
                    "-cp ${classpath}"];
            }

            TryGetIsVanillaAndVersion(ref gameEntity, entity, gameEntity
                .OfVersionJsonPath());

            return gameEntity;
        }

        /// <summary>
        /// 获取所有游戏实体信息
        /// </summary>
        /// <returns></returns>
        public IEnumerable<GameEntry> GetGameEntitys() {
            var versionsPath = new DirectoryInfo(Path.Combine(Root.FullName, "versions"));
            foreach (var item in versionsPath.EnumerateDirectories()) {
                var entry = GetGameEntity(item.Name);
                if (entry is null) {
                    continue;
                }

                yield return entry;
            }
        }

        public GameJsonEntry Resolve(string id) {
            var path = Path.Combine(root, "versions", id, $"{id}.json");
            if (!File.Exists(path)) {
                return null!;
            }

            try {
                var json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<GameJsonEntry>(json)!;
            }
            catch {
                throw new GameResolveFailedException($"[{id}]解析失败");
            }
        }

        private IEnumerable<string> HandleMinecraftArguments(string minecraftArguments)
            => GroupArguments(minecraftArguments.Replace("  ", " ").Split(' '));

        private IEnumerable<string> HandleGameArguments(ArgumentsJsonEntry entity)
            => GroupArguments(entity.Game.Where(x => x.ValueKind == JsonValueKind.String).Select(x => x.GetString()!.ToPath()));

        private IEnumerable<string> HandleJvmArguments(ArgumentsJsonEntry entity)
            => GroupArguments(entity.Jvm.Where(x => x.ValueKind == JsonValueKind.String).Select(x => x.GetString()!.ToPath()));

        private static IEnumerable<string> GroupArguments(IEnumerable<string> arguments) {
            List<string> cache = new List<string>();
            string lastArgument = arguments.LastOrDefault()!;

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

        private void TryGetIsVanillaAndVersion(ref GameEntry entity, GameJsonEntry jsonEntity, string path) {
            entity.IsVanilla = IsVanilla(jsonEntity);
            entity.Version = GetVersion(entity, jsonEntity, path);

            bool IsVanilla(GameJsonEntry jsonEntity) {
                var isMainClassValid = jsonEntity.MainClass switch {
                    "net.minecraft.client.main.Main"
                    or "net.minecraft.launchwrapper.Launch"
                    or "com.mojang.rubydung.RubyDung" => true,
                    _ => false,
                };

                var hasTweakClass = jsonEntity.MinecraftArguments?.Contains("--tweakClass") ?? false;
                var hasAlphaVanillaTweaker = jsonEntity.MinecraftArguments
                    ?.Contains("--tweakClass net.minecraft.launchwrapper.AlphaVanillaTweaker") ?? false;

                var hasTweakClassArgument = jsonEntity.Arguments?.Game
                    ?.Any(e => e.ValueKind.Equals(JsonValueKind.String)
                    && e.GetString().Equals("--tweakClass")) ?? false;

                return string.IsNullOrEmpty(jsonEntity.InheritsFrom)
                    && isMainClassValid
                    && (!hasTweakClass || hasAlphaVanillaTweaker)
                    && !hasTweakClassArgument;
            }

            string GetVersion(GameEntry gameInfo, GameJsonEntry jsonEntity, string path) {
                if (gameInfo.IsVanilla) {
                    return gameInfo.Id;
                } else if (gameInfo.IsInheritedFrom) {
                    return gameInfo.InheritsFrom.Id;
                } else {
                    var json = JsonNode.Parse(File.ReadAllText(path));

                    var patches = json["patches"]; // hmcl合并核心版本号读取
                    var clientVersion = json["clientVersion"]; // pcl合并核心版本号读取

                    if (patches != null) {
                        return patches[0]["version"].GetValue<string>();
                    }
                    if (clientVersion != null) {
                        return clientVersion.GetValue<string>();
                    }
                }

                return null;
            }
        }
    }
}
