using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MinecraftLaunch.Modules.Interface;
using MinecraftLaunch.Modules.Models.Download;
using MinecraftLaunch.Modules.Models.Install;
using MinecraftLaunch.Modules.Models.Launch;
using MinecraftLaunch.Modules.Utils;
using Newtonsoft.Json;
using System.Text.Json;
using MinecraftLaunch.Modules.Parser;

namespace MinecraftLaunch.Modules.Installer {
    public partial class GameCoreInstaller : InstallerBase<InstallerResponse> {
        public override async ValueTask<InstallerResponse> InstallAsync() {
            try {
                InvokeStatusChangedEvent(0.1f, "正在获取 游戏核心Json");
                GameCoreJsonEntity entity = JsonConvert.DeserializeObject<GameCoreJsonEntity>(await HttpUtil.GetStringAsync(CoreInfo.Url))!;
                if (!string.IsNullOrEmpty(CustomId)) {
                    entity.Id = CustomId;
                } else {
                    entity.Id = Id;
                }

                InvokeStatusChangedEvent(0.15f, "正在下载 游戏核心Json");
                FileInfo fileInfo = new FileInfo(Path.Combine(GameCoreToolkit.Root!.FullName, "versions", CustomId ?? Id, (CustomId ?? Id) + ".json"));
                if (!fileInfo.Directory!.Exists) {
                    fileInfo.Directory.Create();
                }

                await File.WriteAllTextAsync(fileInfo.FullName, entity.ToJson(), default);

                InvokeStatusChangedEvent(0.3f, "正在下载 游戏依赖资源");
                await new ResourceInstaller(GameCoreToolkit.GetGameCore(CustomId ?? Id)).DownloadAsync((a, e) => {
                    InvokeStatusChangedEvent(0.2f + e * 0.8f, "下载中 " + a);
                });

                InvokeStatusChangedEvent(1f, "安装完成");
                return new InstallerResponse {
                    Success = true,
                    GameCore = GameCoreToolkit.GetGameCore(CustomId ?? Id),
                    Exception = null!
                };
            }
            catch (Exception exception) {
                return new InstallerResponse {
                    Success = false,
                    GameCore = null!,
                    Exception = exception
                };
            }
        }

        public static async ValueTask<GameCoresEntity> GetGameCoresAsync() {
            return (await HttpUtil.GetStringAsync(APIManager.Current.VersionManifest)).ToJsonEntity<GameCoresEntity>();
        }
    }

    partial class GameCoreInstaller {
        public GameCoreInstaller(GameCoreUtil gameCoreToolkit, string Id, string customId = default!) {
            GameCoreToolkit = gameCoreToolkit;
            this.Id = Id;
            GetGameCoresAsync().Result.Cores.ToList().ForEach(x => {
                if (x.Id == Id)
                    this.CoreInfo = x;
            });

            CustomId = customId;
        }

        public GameCoreUtil GameCoreToolkit { get; set; }

        public GameCoreEmtity CoreInfo { get; set; }

        public string CustomId { get; set; } = string.Empty;

        public string Id { get; set; }
    }
}