using MinecraftLaunch.Modules.Enum;
using MinecraftLaunch.Modules.Interface;
using MinecraftLaunch.Modules.Models.Launch;
using MinecraftLaunch.Modules.Utils;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftLaunch.Modules.Analyzers {
    public class GameLatentErrorAnalyzer : IAnalyzer<IEnumerable<LatentErrorInfo>> {
        public GameCore Core { get; private set; }

        public LaunchConfig Config { get; private set; }

        public GameLatentErrorAnalyzer(GameCore core, LaunchConfig config) {
            Core = core;
            Config = config;
        }

        public async ValueTask<IEnumerable<LatentErrorInfo>> AnalyseAsync() {
            List<LatentErrorInfo> result = new();

            #region Jvm
            var jvavInfo = JavaUtil.GetJavaInfo(Config.JvmConfig.JavaPath.FullName);

            if (jvavInfo.JavaSlugVersion != Core.JavaVersion) {
                result.Add(LatentErrorInfo.Build("Java 版本与游戏不一致，可能无法启动游戏", LatentErrorType.Error));
            }

            if (!jvavInfo.Is64Bit) {
                result.Add(LatentErrorInfo.Build("使用的为 32 位 Java，不推荐使用，尽管能正常启动游戏", LatentErrorType.Warning));
            }

            if (!jvavInfo.Is64Bit && Config.JvmConfig.MaxMemory > 1024) {
                result.Add(LatentErrorInfo.Build("使用的为 32 位 Java，且内存 超过最大限制，无法启动游戏", LatentErrorType.Error));
            }
            #endregion

            #region Modpack
            ModPackUtil util = new(Core, Config.IsEnableIndependencyCore);

            var followupModpack = await Task.Run(async () => {
                var modpacks = (await util.LoadAllAsync()).Where(x => x.IsEnabled);

                foreach (var x in modpacks)
                {
                    var modloader = GetModpackModLoader(x.Path);
                    bool isModLoaderAlike = Core.ModLoaderInfos.Any(x => x.ModLoaderType == modloader);
                    bool isMcVersionAlike = Core.Source == x.GameVersion;

                    if (!isMcVersionAlike) {
                        result.Add(LatentErrorInfo.Build($"模组 \"{x.FileName}\" 与此游戏核心版本不同，可能导致游戏无法正常启动", LatentErrorType.Warning));
                    }

                    if (!isModLoaderAlike) {
                        result.Add(LatentErrorInfo.Build($"模组 \"{x.FileName}\" 与此游戏核心安装的模组加载器类型不同，可能导致游戏无法正常启动", LatentErrorType.Error));
                    }
                }

                return modpacks.GroupBy(i => i.Id).Where(g => g.Count() > 1);
            });

            if (followupModpack.Count() > 0) {
                foreach (var item in followupModpack) {
                    LatentErrorInfo.Build($"模组 \"{item.ToList().First().FileName}\" 在此游戏核心已有另一版本，可能导致游戏无法正常启动", LatentErrorType.Error);
                }
            }


            #endregion

            return result;

            ModLoaderType GetModpackModLoader(string filePath) {
                ModLoaderType result = ModLoaderType.Any;

                using var zip = ZipFile.OpenRead(filePath);
                zip.Entries.Where(x => {
                    if (x.FullName.Contains("quilt.mod.json")) {
                        result = ModLoaderType.Quilt;                        
                    } else if (x.FullName.Contains("mcmod.info")) {
                        result = ModLoaderType.Forge;
                    } else if (x.FullName.Contains("fabric.mod.json")) {
                        result = ModLoaderType.Fabric;
                    }

                    return true;
                });

                return result;
            }
        }
    }
}
