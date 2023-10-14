using System.Diagnostics;
using System.IO.Compression;
using MinecraftLaunch.Modules.Enum;
using MinecraftLaunch.Modules.Interface;
using MinecraftLaunch.Modules.Models.Download;
using MinecraftLaunch.Modules.Models.Install;
using MinecraftLaunch.Modules.Utilities;
using System.Text.Json;
using Flurl.Http;
using MinecraftLaunch.Modules.Downloaders;

namespace MinecraftLaunch.Modules.Installer {
    public class OptiFineInstaller : InstallerBase<InstallerResponse> {
        public string CustomId { get; private set; }

        public GameCoreUtil GameCoreLocator { get; private set; }

        public string JavaPath { get; private set; }

        public OptiFineInstallEntity OptiFineBuild { get; private set; }

        public string PackageFile { get; private set; }

        public InstallType InstallType { get; private set; }

        public override async ValueTask<InstallerResponse> InstallAsync() {
            #region Download Package

            InvokeStatusChangedEvent(0.0f, "开始下载 Optifine 安装包");

            if (string.IsNullOrEmpty(PackageFile) || !File.Exists(PackageFile)) {
                var downloadResponse = await DownloadOptiFinePackageFromBuildAsync(this.OptiFineBuild, GameCoreLocator.Root!, (progress, message) => {
                    InvokeStatusChangedEvent(0.15f * progress, $"开始下载 Optifine 安装包中 {message}");
                });

                if (downloadResponse.HttpStatusCode != System.Net.HttpStatusCode.OK) {
                    throw new HttpRequestException(downloadResponse.HttpStatusCode.ToString());
                }

                PackageFile = downloadResponse.FileInfo.FullName;
            }

            #endregion

            #region Parse Package

            InvokeStatusChangedEvent(0.45f, "开始解析 OptiFine 安装包");

            using var archive = ZipFile.OpenRead(PackageFile);
            string launchwrapper = "1.12";

            if (archive.GetEntry("launchwrapper-of.txt") != null) {
                launchwrapper = archive.GetEntry("launchwrapper-of.txt").GetString();
            }

            #endregion

            #region Check Inherited Core

            InvokeStatusChangedEvent(0.55f, "开始检查继承的核心");
            if (GameCoreLocator.GetGameCore(OptiFineBuild.McVersion) == null) {
                var installer = new GameCoreInstaller(GameCoreLocator, OptiFineBuild.McVersion);
                installer.ProgressChanged += (_, e) => {
                    InvokeStatusChangedEvent(0.45f + 0.15000004f * e.Progress, "正在下载继承的游戏核心：" + e.ProgressDescription);
                };

                await installer.InstallAsync();
            }

            #endregion

            #region Write Files

            InvokeStatusChangedEvent(0.7f, "开始写入文件");

            var entity = new OptiFineGameCoreJsonEntity {
                Id = string.IsNullOrEmpty(CustomId) ? $"{OptiFineBuild.McVersion}-OptiFine_{OptiFineBuild.Type}_{OptiFineBuild.Patch}" : CustomId,
                InheritsFrom = OptiFineBuild.McVersion,
                Time = DateTime.Now.ToString("O"),
                ReleaseTime = DateTime.Now.ToString("O"),
                Type = "release",
                Libraries = new() {
                    new Models.Launch.LibraryJsonEntity {
                        Name = $"optifine:Optifine:{OptiFineBuild.McVersion}_{OptiFineBuild.Type}_{OptiFineBuild.Patch}"
                    },
                    new Models.Launch.LibraryJsonEntity {
                        Name = (launchwrapper.Equals("1.12") ? "net.minecraft:launchwrapper:1.12" : ("optifine:launchwrapper-of:" + launchwrapper))
                    }
                },
                MainClass = "net.minecraft.launchwrapper.Launch",
                Arguments = new ArgumentsJsonEntity {
                    Game = new() {
                        JsonDocument.Parse("\"--tweakClass\"").RootElement,
                        JsonDocument.Parse("\"optifine.OptiFineTweaker\"").RootElement
                    }
                }
            };

            var coreJsonFile = new FileInfo(Path.Combine(GameCoreLocator.Root!.FullName, "versions", entity.Id, $"{entity.Id}.json"));

            if (!coreJsonFile.Directory!.Exists) {
                coreJsonFile.Directory.Create();
            }

            await File.WriteAllTextAsync(coreJsonFile.FullName, entity.ToJson());

            var launchwrapperFile = new LibraryResource() { 
                Name = entity.Libraries[1].Name, Root = this.GameCoreLocator.Root 
            }.ToFileInfo();

            if (!launchwrapper.Equals("1.12")) {
                if (!launchwrapperFile.Directory!.Exists) {
                    launchwrapperFile.Directory.Create();
                }

                archive.GetEntry($"launchwrapper-of-{launchwrapper}.jar")!.ExtractToFile(launchwrapperFile.FullName, true);
            } else if (!launchwrapperFile.Exists) {
                await FileDownloader.DownloadAsync(new LibraryResource() {
                    Name = entity.Libraries[1].Name, Root = this.GameCoreLocator.Root 
                }.ToDownloadRequest());
            }

            string inheritsFromFile = Path.Combine(GameCoreLocator.Root.FullName, "versions", OptiFineBuild.McVersion, $"{OptiFineBuild.McVersion}.jar");
            File.Copy(inheritsFromFile, Path.Combine(coreJsonFile.Directory.FullName, $"{entity.Id}.jar"), true);

            var optiFineLibraryFile = new LibraryResource { 
                Name = entity.Libraries[0].Name, Root = this.GameCoreLocator.Root 
            }.ToFileInfo();

            if (!optiFineLibraryFile.Directory!.Exists) {
                optiFineLibraryFile.Directory.Create();
            }

            #endregion

            #region Run Processor

            InvokeStatusChangedEvent(0.85f, "运行安装程序处理器中");

            var process = Process.Start(new ProcessStartInfo(JavaPath) {
                UseShellExecute = false,
                WorkingDirectory = this.GameCoreLocator.Root.FullName,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                Arguments = string.Join(" ", new string[]
                {
                "-cp",
                PackageFile,
                "optifine.Patcher",
                inheritsFromFile,
                PackageFile,
                optiFineLibraryFile.FullName
                })
            });

            var outputs = new List<string>();
            var errorOutputs = new List<string>();

            process.OutputDataReceived += (_, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                    outputs.Add(args.Data);
            };
            process.ErrorDataReceived += (_, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data)) {
                    outputs.Add(args.Data);
                    errorOutputs.Add(args.Data);
                }
            };

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.WaitForExit();

            #endregion

            #region ModpackTypeInstaller Exit

            if (InstallType is InstallType.Modpack) {
                var gameCore = GameCoreLocator.GetGameCore(entity.Id);

                string directoryPath = gameCore.GetModsPath();
                directoryPath.IsDirectory(true);

                var result = optiFineLibraryFile.CopyTo(Path.Combine(directoryPath, optiFineLibraryFile.Name), true);
                InvokeStatusChangedEvent(1f, "安装完成");

                return new() {
                    Success = result.Exists,
                    GameCore = GameCoreLocator.GetGameCore(entity.Id),
                    Exception = null!
                };
            }

            #endregion

            InvokeStatusChangedEvent(1f, "安装完成");

            return new() {
                Success = true,
                GameCore = GameCoreLocator.GetGameCore(entity.Id),
                Exception = null!
            };
        }

        public static async ValueTask<OptiFineInstallEntity[]> GetOptiFineBuildsFromMcVersionAsync(string mcVersion) {
            try {
                string url = $"{(APIManager.Current.Host.Equals(APIManager.Mojang.Host) ? APIManager.Bmcl.Host : APIManager.Current.Host)}/optifine/{mcVersion}";
                using var responseMessage = await url.GetAsync();
                responseMessage.ResponseMessage.EnsureSuccessStatusCode();

                var list = JsonSerializer.Deserialize<List<OptiFineInstallEntity>>(await responseMessage.GetStringAsync());

                var preview = list!.Where(x => x.Patch.StartsWith("pre")).ToList();
                var release = list!.Where(x => !x.Patch.StartsWith("pre")).ToList();

                release.Sort((a, b) => $"{a.Type}_{a.Patch}".CompareTo($"{b.Type}_{b.Patch}"));
                preview.Sort((a, b) => $"{a.Type}_{a.Patch}".CompareTo($"{b.Type}_{b.Patch}"));

                var builds = preview.Union(release).ToList();
                builds.Reverse();

                return builds.ToArray();
            }
            catch {
                return Array.Empty<OptiFineInstallEntity>();
            }
        }

        public static ValueTask<HttpDownloadResponse> DownloadOptiFinePackageFromBuildAsync(OptiFineInstallEntity build, DirectoryInfo directory, Action<float, string> progressChangedAction) {
            string downloadUrl = $"{(APIManager.Current.Host.Equals(APIManager.Mojang.Host) ? APIManager.Bmcl.Host : APIManager.Current.Host)}/optifine/{build.McVersion}/{build.Type}/{build.Patch}";
            return HttpUtil.HttpDownloadAsync(new HttpDownloadRequest {
                Url = downloadUrl,
                Directory = directory
            }, progressChangedAction);
        }

        public OptiFineInstaller(GameCoreUtil coreLocator, OptiFineInstallEntity build, string javaPath, InstallType installType = InstallType.GameCore, string packageFile = null!, string customId = null!) {
            OptiFineBuild = build;
            JavaPath = javaPath;
            PackageFile = packageFile;
            GameCoreLocator = coreLocator;
            CustomId = customId;
            InstallType = installType;
        }
    }
}
