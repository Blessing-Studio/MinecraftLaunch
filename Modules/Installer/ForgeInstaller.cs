using System.Diagnostics;
using System.IO.Compression;
using MinecraftLaunch.Modules.Interface;
using MinecraftLaunch.Modules.Models.Download;
using MinecraftLaunch.Modules.Models.Install;
using MinecraftLaunch.Modules.Models.Launch;
using MinecraftLaunch.Modules.Parser;
using MinecraftLaunch.Modules.Utilities;
using System.Text.Json;
using System.Text.Json.Nodes;
using MinecraftLaunch.Modules.Downloaders;
using Flurl.Http;
using System;

namespace MinecraftLaunch.Modules.Installer {
    public partial class ForgeInstaller : InstallerBase<InstallerResponse> {
        private GameCoreJsonEntity GetGameCoreJsonEntity(ZipArchive archive, JsonDocument installProfile) {
            if (installProfile.RootElement.TryGetProperty("versionInfo", out var versionInfo)) {
                return JsonSerializer.Deserialize<GameCoreJsonEntity>(versionInfo.GetRawText())!;
            }

            ZipArchiveEntry entry = archive.GetEntry("version.json")!;
            if (entry != null) {
                return JsonSerializer.Deserialize<GameCoreJsonEntity>(ExtendUtil.GetString(entry))!;
            }
            return null!;
        }
        
        private string CombineLibraryName(string name) {
            string libraries = Path.Combine(GameCoreLocator.Root.FullName, "libraries");
            foreach (string subPath in LibraryResource.FormatName(name.TrimStart('[').TrimEnd(']'))) {
                libraries = Path.Combine(libraries, subPath);
            }
            return libraries;
        }

        public override async ValueTask<InstallerResponse> InstallAsync() {
            #region Download Package
            InvokeStatusChangedEvent(0f, "开始下载 Forge 安装包");
            if (string.IsNullOrEmpty(PackageFile) || !File.Exists(PackageFile)) {
                var downloadResponse = await DownForgeOfBuildAsync(this.ForgeBuild.Build, GameCoreLocator.Root, (progress, message) => {
                    InvokeStatusChangedEvent(0.1d * progress, "下载 Forge 安装包中");
                });

                if (downloadResponse.HttpStatusCode != System.Net.HttpStatusCode.OK)
                    throw new HttpRequestException(downloadResponse.HttpStatusCode.ToString());

                PackageFile = downloadResponse.Result
                    .FullName;
            }

            #endregion

            #region Parse Package
            InvokeStatusChangedEvent(0.15f, "开始解析 Forge 安装包");
            using ZipArchive archive = ZipFile.OpenRead(PackageFile);
            var installProfile = JsonDocument.Parse(ExtendUtil.GetString(archive.GetEntry("install_profile.json")));

            GameCoreJsonEntity entity = GetGameCoreJsonEntity(archive, installProfile);
            IEnumerable<LibraryResource> libraries = new LibraryParser(entity.Libraries, GameCoreLocator.Root!).GetLibraries();
            IEnumerable<LibraryResource> enumerable;
            if (!installProfile.RootElement.TryGetProperty("libraries", out var librariesElement)) {
                enumerable = Array.Empty<LibraryResource>().AsEnumerable();
            } else {
                IEnumerable<LibraryResource> libraries2 = new LibraryParser(JsonSerializer.Deserialize<IEnumerable<Models.Launch.LibraryJsonEntity>>(librariesElement.GetRawText())!.ToList(), GameCoreLocator.Root).GetLibraries();
                enumerable = libraries2;
            }
            IEnumerable<LibraryResource> installerLibraries = enumerable;
            Dictionary<string, JsonNode> dataDictionary;
            if (installProfile.RootElement.TryGetProperty("data", out var dataElement)) {
                dataDictionary = JsonSerializer.Deserialize<Dictionary<string, JsonNode>>(dataElement.GetRawText())!;
            } else {
                dataDictionary = new Dictionary<string, JsonNode>();
            }

            IEnumerable<LibraryResource> downloadLibraries = libraries.Union(installerLibraries);
            if (!string.IsNullOrEmpty(CustomId))
                entity.Id = CustomId;
            #endregion

            #region Download Libraries
            try {
                InvokeStatusChangedEvent(0.15f, "开始下载 Forge 依赖文件");
                var downloader = new MultithreadedDownloader<LibraryResource>(x => x.ToDownloadRequest(), downloadLibraries.ToList());
                downloader.ProgressChanged += (object sender, (float, string) e) => InvokeStatusChangedEvent(0.15f + 0.45f * e.Item1, "下载 Forge 依赖文件中：" + e.Item2);

                if (APIManager.Current.Host.Equals(APIManager.Mojang.Host)) {
                    APIManager.Current = APIManager.Bmcl;
                    downloader.Completed += delegate { APIManager.Current = APIManager.Mojang; };
                }

                var multithreadedDownload = await downloader.DownloadAsync();
            }
            catch (Exception) {
            }
            #endregion

            #region Write Files
            InvokeStatusChangedEvent(0.7f, "开始写入文件");
            string forgeFolderId = $"{ForgeBuild.McVersion}-{ForgeBuild.ForgeVersion}";
            string forgeLibrariesFolder = Path.Combine(GameCoreLocator.Root.FullName, "libraries", "net", "minecraftforge", "forge", forgeFolderId);

            if (installProfile.RootElement.TryGetProperty("install", out var versionInfo)) {
                var lib = new LibraryResource {
                    Root = GameCoreLocator.Root,//
                    Name = installProfile.RootElement.GetProperty("install").GetProperty("path").ToString()
                };

                archive.GetEntry(installProfile.RootElement.GetProperty("install").GetProperty("filePath")!.ToString()).ExtractTo(lib.ToFileInfo().FullName);
            }

            if (archive.GetEntry("maven/") != null) {
                archive.GetEntry($"maven/net/minecraftforge/forge/{forgeFolderId}/forge-{forgeFolderId}.jar")?
                    .ExtractTo(Path.Combine(forgeLibrariesFolder, $"forge-{forgeFolderId}.jar"));
                archive.GetEntry($"maven/net/minecraftforge/forge/{forgeFolderId}/forge-{forgeFolderId}-universal.jar")?
                    .ExtractTo(Path.Combine(forgeLibrariesFolder, $"forge-{forgeFolderId}-universal.jar"));
            }

            if (dataDictionary!.Any()) {
                archive.GetEntry("data/client.lzma").ExtractTo(Path.Combine(forgeLibrariesFolder, $"forge-{forgeFolderId}-clientdata.lzma"));
                archive.GetEntry("data/server.lzma").ExtractTo(Path.Combine(forgeLibrariesFolder, $"forge-{forgeFolderId}-serverdata.lzma"));
            }

            var versionJsonFile = new FileInfo(Path.Combine(GameCoreLocator.Root.FullName, "versions", entity.Id, $"{entity.Id}.json"));

            if (!versionJsonFile.Directory!.Exists)
                versionJsonFile.Directory.Create();

            File.WriteAllText(versionJsonFile.FullName, entity.ToJson());

            #endregion

            #region Check Inherited Core
            InvokeStatusChangedEvent(0.7f, "开始检查继承的核心");
            if (GameCoreLocator.GetGameCore(ForgeBuild.McVersion) == null) {
                var installer = new GameCoreInstaller(GameCoreLocator, ForgeBuild.McVersion);
                installer.ProgressChanged += (_, e) => {
                    InvokeStatusChangedEvent(0.7f + (0.85f - 0.7f) * e.Progress, "正在下载继承的游戏核心：" + e.ProgressDescription);
                };

                await installer.InstallAsync();
            }
            #endregion

            #region LegacyForgeInstaller Exit
            if (installProfile.RootElement.TryGetProperty("versionInfo", out var element) || dataDictionary.Count == 0) {
                InvokeStatusChangedEvent(1f, "安装完成");
                return new InstallerResponse {
                    Success = true,
                    Exception = null!,
                    GameCore = GameCoreLocator.GetGameCore(entity.Id)
                };
            }

            #endregion

            #region Parser Processor
            InvokeStatusChangedEvent(0.85f, "开始分析安装处理器");
            try {
                dataDictionary!["BINPATCH"]["client"] = $"[net.minecraftforge:forge:{forgeFolderId}:clientdata@lzma]";
                dataDictionary["BINPATCH"]["server"] = $"[net.minecraftforge:forge:{forgeFolderId}:serverdata@lzma]";
            }
            catch (Exception) { }

            var replaceValues = new Dictionary<string, string>
            {
                { "{SIDE}", "client" },
                { "{MINECRAFT_JAR}", Path.Combine(this.GameCoreLocator.Root.FullName, "versions", this.ForgeBuild.McVersion, $"{this.ForgeBuild.McVersion}.jar") },
                { "{MINECRAFT_VERSION}", this.ForgeBuild.McVersion },
                { "{ROOT}", this.GameCoreLocator.Root.FullName },
                { "{INSTALLER}", this.PackageFile },
                { "{LIBRARY_DIR}", Path.Combine(this.GameCoreLocator.Root.FullName, "libraries") }
            };

            var replaceProcessorArgs = dataDictionary!.ToDictionary(x => $"{{{x.Key}}}", x => {
                string value = x.Value["client"]!.ToString();

                if (value.StartsWith("[") && value.EndsWith("]"))
                    return CombineLibraryName(value);

                return value;
            });

            var processors = JsonSerializer.Deserialize<IEnumerable<ForgeInstallProcessorEntity>>(installProfile.RootElement.GetProperty("processors").GetRawText())!
                .Where(x => {
                    if (!x.Sides.Any())
                        return true;

                    return x.Sides.Contains("client");
                }).Select(x => {
                    x.Args = x.Args.Select(y => {
                        if (y.StartsWith("[") && y.EndsWith("]"))
                            return CombineLibraryName(y);

                        return y.Replace(replaceProcessorArgs).Replace(replaceValues);
                    }).ToList();

                    x.Outputs = x.Outputs.Select(kvp => (kvp.Key.Replace(replaceProcessorArgs), kvp.Value.Replace(replaceProcessorArgs))).ToDictionary(z => z.Item1, z => z.Item2);

                    return x;
                }).ToList();
            #endregion

            #region Run Processor
            var processes = new Dictionary<List<string>, List<string>>();

            foreach (var forgeInstallProcessor in processors) {
                var fileName = CombineLibraryName(forgeInstallProcessor.Jar);
                using var fileArchive = ZipFile.OpenRead(fileName);

                string mainClass = fileArchive.GetEntry("META-INF/MANIFEST.MF").GetString().Split("\r\n".ToCharArray()).First(x => x.Contains("Main-Class: ")).Replace("Main-Class: ", string.Empty);
                string classPath = string.Join(Path.PathSeparator.ToString(), new List<string> { forgeInstallProcessor.Jar }
                    .Concat(forgeInstallProcessor.Classpath)
                    .Select(x => new LibraryResource { Name = x, Root = GameCoreLocator.Root })
                    .Select(x => x.ToFileInfo().FullName));

                var args = new List<string>
                {
                    "-cp",
                    $"\"{classPath}\"",
                    mainClass
                };

                args.AddRange(forgeInstallProcessor.Args);

                using var process = Process.Start(new ProcessStartInfo(JavaPath) {
                    Arguments = string.Join(' '.ToString(), args),
                    UseShellExecute = false,
                    WorkingDirectory = this.GameCoreLocator.Root.FullName,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                });

                var outputs = new List<string>();
                var errorOutputs = new List<string>();

                process!.OutputDataReceived += (_, args) => {
                    if (!string.IsNullOrEmpty(args.Data))
                        outputs.Add(args.Data);
                };
                process.ErrorDataReceived += (_, args) => {
                    if (!string.IsNullOrEmpty(args.Data)) {
                        outputs.Add(args.Data);
                        errorOutputs.Add(args.Data);
                    }
                };

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                process.WaitForExit();
                processes.Add(outputs, errorOutputs);

                InvokeStatusChangedEvent(0.85f + 0.15f * ((float)processors.IndexOf(forgeInstallProcessor) / (float)processors.Count), $"运行安装程序处理器中：{processors.IndexOf(forgeInstallProcessor)}/{processors.Count}");
            }
            #endregion

            InvokeStatusChangedEvent(1f, "安装完成");
            return new InstallerResponse {
                Success = true,
                Exception = null!,
                GameCore = GameCoreLocator.GetGameCore(entity.Id)
            };
        }

        public static async ValueTask<ForgeInstallEntity[]> GetForgeBuildsOfVersionAsync(string mcVersion) {
            try {
                string url = $"{(APIManager.Current.Host.Equals(APIManager.Mojang.Host) ? APIManager.Bmcl.Host : APIManager.Current.Host)}/forge/minecraft/{mcVersion}";
                using var responseMessage = await url.GetAsync();
                responseMessage.ResponseMessage.EnsureSuccessStatusCode();
                await Console.Out.WriteLineAsync($"{(APIManager.Current.Host.Equals(APIManager.Mojang.Host) ? APIManager.Bmcl.Host : APIManager.Current.Host)}/forge/minecraft/{mcVersion}");
                var list = JsonSerializer.Deserialize<List<ForgeInstallEntity>>(await responseMessage.GetStringAsync());

                list.Sort((a, b) => a.Build.CompareTo(b.Build));
                list.Reverse();

                return list.ToArray();
            }
            catch {
                return Array.Empty<ForgeInstallEntity>();
            }
        }

        public static async ValueTask<FileDownloaderResponse> DownForgeOfBuildAsync(int build, DirectoryInfo directory, Action<double, string> progressChangedAction) {
            var downloadUrl = $"{(APIManager.Current.Host.Equals(APIManager.Mojang.Host) ? APIManager.Bmcl.Host : APIManager.Current.Host)}/forge/download/{build}";
            using var downloader = FileDownloader.Build(new DownloadRequest {
                Url = downloadUrl,
                Directory = directory
            });

            downloader.DownloadProgressChanged += (_, x) => {
                progressChangedAction(x.Progress, x.Progress.ToString());
            };

            downloader.BeginDownload();            
            return await downloader.CompleteAsync();
        }
    }

    partial class ForgeInstaller {
        public string CustomId { get; private set; }

        public string JavaPath { get; private set; }

        public ForgeInstallEntity ForgeBuild { get; private set; }

        public string PackageFile { get; private set; }

        public GameCoreUtil GameCoreLocator { get; private set; }
    }

    partial class ForgeInstaller {
        public ForgeInstaller(GameCoreUtil coreLocator, ForgeInstallEntity build, string javaPath, string customId = null, string packageFile = null) {
            ForgeBuild = build;
            JavaPath = javaPath;
            PackageFile = packageFile;
            GameCoreLocator = coreLocator;
            CustomId = customId;
        }
    }
}