using System.Diagnostics;
using System.IO.Compression;
using MinecraftLaunch.Modules.Interface;
using MinecraftLaunch.Modules.Models.Download;
using MinecraftLaunch.Modules.Models.Install;
using MinecraftLaunch.Modules.Utils;
using Natsurainko.Toolkits.IO;
using Natsurainko.Toolkits.Network;
using Natsurainko.Toolkits.Network.Model;
using Newtonsoft.Json;

namespace MinecraftLaunch.Modules.Installer {
    public class OptiFineInstaller : InstallerBase<InstallerResponse>
    {
        public string CustomId { get; private set; }

        public GameCoreUtil GameCoreLocator { get; private set; }

        public string JavaPath { get; private set; }

        public OptiFineInstallEntity OptiFineBuild { get; private set; }

        public string PackageFile { get; private set; }

        public override async ValueTask<InstallerResponse> InstallAsync()
        {
            #region Download PackageFile
            InvokeStatusChangedEvent(0f, "开始下载 OptiFine 安装包");
            if (string.IsNullOrEmpty(PackageFile) || !File.Exists(PackageFile))
            {
                var downloadResponse = await DownloadOptiFinePackageFromBuildAsync(this.OptiFineBuild, GameCoreLocator.Root, (progress, message) =>
                {
                    InvokeStatusChangedEvent(0.15f * progress, "下载 OptiFine 安装包中 " + message);
                });

                if (downloadResponse.HttpStatusCode != System.Net.HttpStatusCode.OK)
                    throw new HttpRequestException(downloadResponse.HttpStatusCode.ToString());

                PackageFile = downloadResponse.FileInfo.FullName;
            }
            #endregion

            InvokeStatusChangedEvent(0.45f, "开始解析 OptiFine 安装包");
            using (ZipArchive archive = ZipFile.OpenRead(PackageFile))
            {
                string launchwrapper = "1.12";
                if (archive.GetEntry("launchwrapper-of.txt") != null)
                {
                    launchwrapper = ZipExtension.GetString(archive.GetEntry("launchwrapper-of.txt"));
                }
                InvokeStatusChangedEvent(0.55f, "开始检查继承的核心");
                if (GameCoreLocator.GetGameCore(OptiFineBuild.McVersion) == null)
                {
                    var installer = new GameCoreInstaller(GameCoreLocator, OptiFineBuild.McVersion);
                    installer.ProgressChanged += (_, e) => {
                        InvokeStatusChangedEvent(0.45f + 0.15000004f * e.Progress, "正在下载继承的游戏核心：" + e.ProgressDescription);
                    };

                    await installer.InstallAsync();
                }

                OptiFineGameCoreJsonEntity entity = new OptiFineGameCoreJsonEntity
                {
                    Id = (string.IsNullOrEmpty(CustomId) ? $"{OptiFineBuild.McVersion}-OptiFine_{OptiFineBuild.Type}_{OptiFineBuild.Patch}" : CustomId),
                    InheritsFrom = OptiFineBuild.McVersion,
                    Time = DateTime.Now.ToString("O"),
                    ReleaseTime = DateTime.Now.ToString("O"),
                    Type = "release",
                    Libraries = new List<MinecraftLaunch.Modules.Models.Launch.LibraryJsonEntity>
                {
                    new MinecraftLaunch.Modules.Models.Launch.LibraryJsonEntity
                    {
                        Name = $"optifine:Optifine:{OptiFineBuild.McVersion}_{OptiFineBuild.Type}_{OptiFineBuild.Patch}"
                    },
                    new MinecraftLaunch.Modules.Models.Launch.LibraryJsonEntity
                    {
                        Name = (launchwrapper.Equals("1.12") ? "net.minecraft:launchwrapper:1.12" : ("optifine:launchwrapper-of:" + launchwrapper))
                    }
                },
                    MainClass = "net.minecraft.launchwrapper.Launch",
                    Arguments = new MinecraftLaunch.Modules.Models.Install.ArgumentsJsonEntity
                    {
                        Game = new()
                    {
                        "--tweakClass",
                        "optifine.OptiFineTweaker"
                    }
                    }
                };
                InvokeStatusChangedEvent(0.7f, "开始写入文件");
                InvokeStatusChangedEvent(0.75f, "开始分析是否安装模组加载器");
                string id = (string.IsNullOrEmpty(CustomId) ? $"{OptiFineBuild.McVersion}-OptiFine-{OptiFineBuild.Type}_{OptiFineBuild.Patch}" : CustomId);
                bool flag;
                try
                {
                    flag = GameCoreLocator.GetGameCore(id)?.HasModLoader ?? false;
                }
                catch (Exception)
                {
                    flag = false;
                }

                if (!flag)
                {
                    FileInfo versionJsonFile = new FileInfo(Path.Combine(GameCoreLocator.Root.FullName, "versions", entity.Id, entity.Id + ".json"));
                    if (!versionJsonFile.Directory!.Exists)
                    {
                        versionJsonFile.Directory.Create();
                    }
                    File.WriteAllText(versionJsonFile.FullName, entity.ToJson());
                }
                FileInfo launchwrapperFile = new LibraryResource
                {
                    Name = entity.Libraries[1].Name,
                    Root = GameCoreLocator.Root
                }.ToFileInfo();
                if (!launchwrapper.Equals("1.12"))
                {
                    if (!launchwrapperFile.Directory!.Exists)
                    {
                        launchwrapperFile.Directory.Create();
                    }
                    archive.GetEntry("launchwrapper-of-" + launchwrapper + ".jar")!.ExtractToFile(launchwrapperFile.FullName, overwrite: true);
                }
                else if (!launchwrapperFile.Exists)
                {
                    await HttpUtil.HttpDownloadAsync(new LibraryResource
                    {
                        Name = entity.Libraries[1].Name,
                        Root = GameCoreLocator.Root
                    }.ToDownloadRequest());
                }
                string inheritsFromFile = Path.Combine(GameCoreLocator.Root.FullName, "versions", OptiFineBuild.McVersion, OptiFineBuild.McVersion + ".jar");
                string v = Path.Combine(GameCoreLocator.Root.FullName, "versions", id);
                File.Copy(inheritsFromFile, Path.Combine(v, entity.Id + ".jar"), overwrite: true);
                FileInfo optiFineLibraryFiles = new LibraryResource
                {
                    Name = entity.Libraries[0].Name,
                    Root = GameCoreLocator.Root
                }.ToFileInfo();
                string optiFineLibraryFile = optiFineLibraryFiles.FullName;
                if (!optiFineLibraryFiles.Directory!.Exists)
                {
                    optiFineLibraryFiles.Directory.Create();
                }

                InvokeStatusChangedEvent(0.85f,"运行安装程序处理器中");
                using Process process = Process.Start(new ProcessStartInfo(JavaPath)
                {
                    UseShellExecute = false,
                    WorkingDirectory = GameCoreLocator.Root.FullName,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    Arguments = string.Join(" ", "-cp", PackageFile, "optifine.Patcher", inheritsFromFile, PackageFile, optiFineLibraryFile)
                })!;
                List<string> outputs = new List<string>();
                List<string> errorOutputs = new List<string>();
                process.OutputDataReceived += delegate (object _, DataReceivedEventArgs args)
                {
                    if (!string.IsNullOrEmpty(args.Data))
                    {
                        outputs.Add(args.Data);
                    }
                };
                process.ErrorDataReceived += delegate (object _, DataReceivedEventArgs args)
                {
                    if (!string.IsNullOrEmpty(args.Data))
                    {
                        outputs.Add(args.Data);
                        errorOutputs.Add(args.Data);
                    }
                };
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                await Task.Run((Func<bool>)process.WaitForInputIdle);

                if (flag) {               
                    FileInfo fileInfo = new(optiFineLibraryFile);
                    Console.WriteLine(fileInfo.FullName);
                    string mods = Path.Combine(GameCoreLocator.Root.FullName, "versions", id, "mods");
                    if (!Directory.Exists(mods)) {
                   
                        Directory.CreateDirectory(mods);
                    }

                    Path.Combine(mods, fileInfo.Name);

                    try {                   
                        fileInfo.CopyTo(Path.Combine(mods, fileInfo.Name), overwrite: true);
                    }
                    finally
                    {
                        fileInfo.Directory!.Delete(recursive: true);
                    }
                }
                
                InvokeStatusChangedEvent(1f, "安装完成");
                return new InstallerResponse
                {
                    Success = true,
                    GameCore = GameCoreLocator.GetGameCore(id),
                    Exception = null!
                };
            }
        }

        public static async ValueTask<OptiFineInstallEntity[]> GetOptiFineBuildsFromMcVersionAsync(string mcVersion)
        {
            try
            {
                using var responseMessage = await HttpWrapper.HttpGetAsync($"{(APIManager.Current.Host.Equals(APIManager.Mojang.Host) ? APIManager.Bmcl.Host : APIManager.Current.Host)}/optifine/{mcVersion}");
                responseMessage.EnsureSuccessStatusCode();

                var list = JsonConvert.DeserializeObject<List<OptiFineInstallEntity>>(await responseMessage.Content.ReadAsStringAsync());

                var preview = list!.Where(x => x.Patch.StartsWith("pre")).ToList();
                var release = list!.Where(x => !x.Patch.StartsWith("pre")).ToList();

                release.Sort((a, b) => $"{a.Type}_{a.Patch}".CompareTo($"{b.Type}_{b.Patch}"));
                preview.Sort((a, b) => $"{a.Type}_{a.Patch}".CompareTo($"{b.Type}_{b.Patch}"));

                var builds = preview.Union(release).ToList();
                builds.Reverse();

                return builds.ToArray();
            }
            catch
            {
                return Array.Empty<OptiFineInstallEntity>();
            }
        }

        public static ValueTask<HttpDownloadResponse> DownloadOptiFinePackageFromBuildAsync(OptiFineInstallEntity build, DirectoryInfo directory, Action<float, string> progressChangedAction)
        {
            string downloadUrl = $"{(APIManager.Current.Host.Equals(APIManager.Mojang.Host) ? APIManager.Bmcl.Host : APIManager.Current.Host)}/optifine/{build.McVersion}/{build.Type}/{build.Patch}";
            return HttpUtil.HttpDownloadAsync(new HttpDownloadRequest
            {
                Url = downloadUrl,
                Directory = directory
            }, progressChangedAction);
        }

        public OptiFineInstaller(GameCoreUtil coreLocator, OptiFineInstallEntity build, string javaPath, string packageFile = null!, string customId = null!)
        {
            OptiFineBuild = build;
            JavaPath = javaPath;
            PackageFile = packageFile;
            GameCoreLocator = coreLocator;
            CustomId = customId;
        }
    }
}
