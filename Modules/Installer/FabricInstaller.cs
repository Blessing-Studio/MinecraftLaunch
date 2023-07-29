using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using MinecraftLaunch.Modules.Interface;
using MinecraftLaunch.Modules.Models.Download;
using MinecraftLaunch.Modules.Models.Install;
using MinecraftLaunch.Modules.Utils;
using Natsurainko.Toolkits.Network;
using Natsurainko.Toolkits.Network.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace MinecraftLaunch.Modules.Installer {
    public partial class FabricInstaller : InstallerBase<InstallerResponse> {
        public override async ValueTask<InstallerResponse> InstallAsync() {
            #region Parse Build
            InvokeStatusChangedEvent(0.25f, "开始分析生成");
            var libraries = FabricBuild.LauncherMeta.Libraries["common"];

            if (FabricBuild.LauncherMeta.Libraries["common"] != null)
                libraries.AddRange(FabricBuild.LauncherMeta.Libraries["client"]);

            libraries.Insert(0, new() { Name = FabricBuild.Intermediary.Maven });
            libraries.Insert(0, new() { Name = FabricBuild.Loader.Maven });

            string mainClass = FabricBuild.LauncherMeta.MainClass.Type == JTokenType.Object
                ? FabricBuild.LauncherMeta.MainClass.ToObject<Dictionary<string, string>>()["client"]
                : string.IsNullOrEmpty(FabricBuild.LauncherMeta.MainClass.ToString())
                    ? "net.minecraft.client.main.Main"
                    : FabricBuild.LauncherMeta.MainClass.ToString();

            string inheritsFrom = FabricBuild.Intermediary.Version;

            if (mainClass == "net.minecraft.client.main.Main")
                return new() {
                    Success = false,
                    GameCore = null,
                    Exception = new ArgumentNullException("MainClass")
                };
            #endregion

            #region Download Libraries
            InvokeStatusChangedEvent(0.45f, "开始下载依赖文件");
            libraries.ForEach(x => x.Url = UrlExtension.Combine("https://maven.fabricmc.net", UrlExtension.Combine(LibraryResource.FormatName(x.Name).ToArray())));

            var downloader = new MultithreadedDownloader<LibraryResource>
                (x => x.ToDownloadRequest(), libraries.Select(y => new LibraryResource { Root = GameCoreLocator.Root, Name = y.Name, Url = y.Url }).ToList());
            downloader.ProgressChanged += (object sender, (float, string) e) => InvokeStatusChangedEvent(0.45f + 0.25f * e.Item1, "下载依赖文件中：" + e.Item2);

            var multithreadedDownload = await downloader.DownloadAsync();
            #endregion

            #region Check Inherited Core
            InvokeStatusChangedEvent(0.55f, "开始检查继承的核心");
            if (GameCoreLocator.GetGameCore(FabricBuild.Intermediary.Version) == null) {
                var installer = new GameCoreInstaller(GameCoreLocator, FabricBuild.Intermediary.Version);
                installer.ProgressChanged += (_, e) => {
                    InvokeStatusChangedEvent(0.45f + (0.85f - 0.45f) * e.Progress, "正在下载继承的游戏核心：" + e.ProgressDescription);
                };

                await installer.InstallAsync();
            }
            #endregion

            #region Write File
            InvokeStatusChangedEvent(0.85f, "开始写入文件");
            var entity = new FabricGameCoreJsonEntity {
                Id = string.IsNullOrEmpty(CustomId) ? $"fabric-loader-{FabricBuild.Loader.Version}-{FabricBuild.Intermediary.Version}" : CustomId,
                InheritsFrom = inheritsFrom,
                ReleaseTime = DateTime.Now.ToString("O"),
                Time = DateTime.Now.ToString("O"),
                Type = "release",
                MainClass = mainClass,
                Arguments = new() { Jvm = new() { "-DFabricMcEmu= net.minecraft.client.main.Main" } },
                Libraries = libraries
            };

            var versionJsonFile = new FileInfo(Path.Combine(GameCoreLocator.Root.FullName, "versions", entity.Id, $"{entity.Id}.json"));

            if (!versionJsonFile.Directory!.Exists)
                versionJsonFile.Directory.Create();

            File.WriteAllText(versionJsonFile.FullName, entity.ToJson());
            #endregion

            InvokeStatusChangedEvent(1f, "安装完成");
            return new InstallerResponse {
                Success = true,
                GameCore = GameCoreLocator.GetGameCore(entity.Id),
                Exception = null!
            };
        }

        public static async ValueTask<FabricMavenItem[]> GetFabricLoaderMavensAsync() {
            try {
                using HttpResponseMessage responseMessage = await HttpWrapper.HttpGetAsync("https://meta.fabricmc.net/v2/versions/loader");
                responseMessage.EnsureSuccessStatusCode();
                return JsonConvert.DeserializeObject<FabricMavenItem[]>(await responseMessage.Content.ReadAsStringAsync());
            }
            catch {
                return Array.Empty<FabricMavenItem>();
            }
        }

        public static async ValueTask<FabricInstallBuild[]> GetFabricBuildsByVersionAsync(string mcVersion) {
            try {
                using var responseMessage = await HttpWrapper.HttpGetAsync($"https://meta.fabricmc.net/v2/versions/loader/{mcVersion}");
                responseMessage.EnsureSuccessStatusCode();

                var list = JsonConvert.DeserializeObject<List<FabricInstallBuild>>(await responseMessage.Content.ReadAsStringAsync());

                list.Sort((a, b) => new Version(a.Loader.Version.Replace(a.Loader.Separator, ".")).CompareTo(new Version(b.Loader.Version.Replace(b.Loader.Separator, "."))));
                list.Reverse();

                return list.ToArray();
            }
            catch // (Exception ex)
            {
                return Array.Empty<FabricInstallBuild>();
            }
        }
    }

    partial class FabricInstaller {
        public FabricInstallBuild FabricBuild { get; private set; }

        public GameCoreUtil GameCoreLocator { get; private set; }

        public string CustomId { get; private set; }

        public FabricInstaller(GameCoreUtil coreLocator, FabricInstallBuild build, string customId = null) {
            FabricBuild = build;
            GameCoreLocator = coreLocator;
            CustomId = customId;
        }
    }
}