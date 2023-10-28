using Flurl.Http;
using MinecraftLaunch.Modules.Downloaders;
using MinecraftLaunch.Modules.Interface;
using MinecraftLaunch.Modules.Models.Download;
using MinecraftLaunch.Modules.Models.Install;
using MinecraftLaunch.Modules.Models.Launch;
using MinecraftLaunch.Modules.Parser;
using MinecraftLaunch.Modules.Utilities;
using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml;

namespace MinecraftLaunch.Modules.Installer {
    public class NeoForgeInstaller : InstallerBase<InstallerResponse> {
        public static readonly string BaseApi = "https://maven.neoforged.net/";

        public static readonly string VersionApi = $"{BaseApi}releases/net/neoforged/forge/maven-metadata.xml";

        public string CustomId { get; private set; }

        public string JavaPath { get; private set; }

        public string PackageFile { get; private set; }

        public GameCoreUtil GameCoreLocator { get; private set; }

        public NeoForgeInstallEntity NeoForgeInstallEntity { get; private set; }

        public NeoForgeInstaller(GameCoreUtil coreLocator, NeoForgeInstallEntity installEntity, string javaPath, string customId = null!, string packageFile = null!) {
            JavaPath = javaPath;
            PackageFile = packageFile;
            GameCoreLocator = coreLocator;
            CustomId = customId;
            NeoForgeInstallEntity = installEntity;
        }

        public static async IAsyncEnumerable<NeoForgeInstallEntity> GetNeoForgesOfVersionAsync(string mcVersion = "1.20.1") {
            using var responseMessage = await VersionApi.GetAsync();
            string xml = await responseMessage.GetStringAsync();

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            var nodes = xmlDoc.SelectNodes("//metadata/versioning/versions/version");
            foreach (XmlElement node in nodes!.AsParallel()) {
                var mcVersionOrNeoVersions = node.InnerXml.Split('-');
                if (mcVersionOrNeoVersions.FirstOrDefault() == mcVersion) {
                    yield return new() {
                        McVersion = mcVersion,
                        NeoForgeVersion = mcVersionOrNeoVersions.LastOrDefault()!
                    };
                }
            }
        }

        public static async ValueTask<HttpDownloadResponse> DownNeoForgeOfBuildAsync(NeoForgeInstallEntity info, DirectoryInfo directory, Action<float, string> action) {
            string url = $"{BaseApi}releases/net/neoforged/forge/{info.McVersion}-{info.NeoForgeVersion}/forge-{info.McVersion}-{info.NeoForgeVersion}-installer.jar";

            return await HttpUtil.HttpDownloadAsync(new HttpDownloadRequest {
                Url = url,
                Directory = directory
            }, action);
        }

        public override async ValueTask<InstallerResponse> InstallAsync() {
            #region Download Package
            InvokeStatusChangedEvent(0f, "开始下载 NeoForged 安装包");
            if (string.IsNullOrEmpty(PackageFile) || !File.Exists(PackageFile)) {
                var downloadResponse = await DownNeoForgeOfBuildAsync(NeoForgeInstallEntity!, GameCoreLocator.Root!, (progress, message) => {
                    InvokeStatusChangedEvent(0.1f * progress, "下载 NeoForged 安装包中");
                });

                if (downloadResponse.HttpStatusCode != System.Net.HttpStatusCode.OK)
                    throw new HttpRequestException(downloadResponse.HttpStatusCode.ToString());

                PackageFile = downloadResponse.FileInfo.FullName;
            }
            #endregion

            #region Parse Package
            InvokeStatusChangedEvent(0.15f, "开始解析 NeoForged 安装包");
            using ZipArchive archive = ZipFile.OpenRead(PackageFile);
            JsonDocument installProfile = JsonDocument.Parse(ExtendUtil.GetString(archive.GetEntry("install_profile.json")!));
            GameCoreJsonEntity entity = GetGameCoreJsonEntity(archive, installProfile);
            IEnumerable<LibraryResource> libraries = new LibraryParser(entity.Libraries, GameCoreLocator.Root!).GetLibraries();
            IEnumerable<LibraryResource> enumerable;
            if (!installProfile.RootElement.TryGetProperty("libraries", out JsonElement librariesElement)) {
                enumerable = Array.Empty<LibraryResource>().AsEnumerable();
            } else {
                IEnumerable<LibraryResource> libraries2 = new LibraryParser(JsonSerializer.Deserialize<IEnumerable<Models.Launch.LibraryJsonEntity>>(librariesElement.GetRawText())!.ToList(), GameCoreLocator.Root!).GetLibraries();
                enumerable = libraries2;
            }
            IEnumerable<LibraryResource> installerLibraries = enumerable;
            Dictionary<string, JsonNode> dataDictionary = (installProfile.RootElement.TryGetProperty("data", out JsonElement dataElement) ? JsonSerializer.Deserialize<Dictionary<string, JsonNode>>(dataElement.GetRawText()) : new Dictionary<string, JsonNode>())!;
            IEnumerable<LibraryResource> downloadLibraries = libraries.Union(installerLibraries);
            if (!string.IsNullOrEmpty(CustomId))
                entity.Id = CustomId;
            #endregion

            #region Download Libraries
            try {
                InvokeStatusChangedEvent(0.15f, "开始下载 NeoForged 依赖文件");
                var downloader = new MultithreadedDownloader<LibraryResource>(x => x.ToDownloadRequest(), downloadLibraries.ToList());
                downloader.ProgressChanged += (object? sender, (float, string) e) => InvokeStatusChangedEvent(0.15f + 0.45f * e.Item1, "下载 NeoForged 依赖文件中：" + e.Item2);

                var multithreadedDownload = await downloader.DownloadAsync();
            }
            catch (Exception) {
            }
            #endregion

            #region Write Files
            InvokeStatusChangedEvent(0.7f, "开始写入文件");
            string forgeFolderId = $"{NeoForgeInstallEntity.McVersion}-{NeoForgeInstallEntity.NeoForgeVersion}";
            string forgeLibrariesFolder = Path.Combine(GameCoreLocator.Root!.FullName, "libraries", "net", "minecraftforge", "forge", forgeFolderId);

            if (installProfile.RootElement.TryGetProperty("install", out JsonElement installElement)) {
                var lib = new LibraryResource {
                    Root = GameCoreLocator.Root,
                    Name = installElement.GetProperty("path").GetString()!
                };

                archive.GetEntry(installElement.GetProperty("filePath").GetString()!).ExtractTo(lib.ToFileInfo().FullName);
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
            if (GameCoreLocator.GetGameCore(NeoForgeInstallEntity.McVersion) == null) {
                var installer = new GameCoreInstaller(GameCoreLocator, NeoForgeInstallEntity.McVersion);
                installer.ProgressChanged += (_, e) => {
                    InvokeStatusChangedEvent(0.7f + (0.85f - 0.7f) * e.Progress, "正在下载继承的游戏核心：" + e.ProgressDescription);
                };

                await installer.InstallAsync();
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
                { "{MINECRAFT_JAR}", Path.Combine(this.GameCoreLocator.Root.FullName, "versions", this.NeoForgeInstallEntity.McVersion, $"{this.NeoForgeInstallEntity.McVersion}.jar") },
                { "{MINECRAFT_VERSION}", this.NeoForgeInstallEntity.McVersion },
                { "{ROOT}", this.GameCoreLocator.Root.FullName },
                { "{INSTALLER}", this.PackageFile },
                { "{LIBRARY_DIR}", Path.Combine(this.GameCoreLocator.Root.FullName, "libraries") }
            };

            var replaceProcessorArgs = dataDictionary!.ToDictionary(x => $"{{{x.Key}}}", x => {
                string value = x.Value["client"]!.GetValue<string>();

                if (value.StartsWith("[") && value.EndsWith("]"))
                    return CombineLibraryName(value);

                return value;
            });

            var processors = installProfile.RootElement.GetProperty("processors").EnumerateArray()
                .Select(x => JsonSerializer.Deserialize<ForgeInstallProcessorEntity>(x.GetRawText()))
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

        private string CombineLibraryName(string name) {
            string libraries = Path.Combine(GameCoreLocator.Root!.FullName, "libraries");
            foreach (string subPath in LibraryResource.FormatName(name.TrimStart('[').TrimEnd(']'))) {
                libraries = Path.Combine(libraries, subPath);
            }
            return libraries;
        }

        private GameCoreJsonEntity GetGameCoreJsonEntity(ZipArchive archive, JsonDocument installProfile) {
            if (installProfile.RootElement.TryGetProperty("versionInfo", out JsonElement versionInfoElement)) {
                return JsonSerializer.Deserialize<GameCoreJsonEntity>(versionInfoElement.GetRawText())!;
            }
            ZipArchiveEntry entry = archive.GetEntry("version.json")!;
            if (entry != null) {
                return JsonSerializer.Deserialize<GameCoreJsonEntity>(ExtendUtil.GetString(entry))!;
            }
            return null;
        }
    }

    public static class AsyncEnumerableHelper {
        public static async ValueTask<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> values) {
            var result = new List<T>();
            await foreach (var item in values) {
                result.Add(item);
            }

            return result;
        }

        public static async ValueTask<List<T>> AsListAsync<T>(this IAsyncEnumerable<T> values) {
            var result = new List<T>();
            await foreach (var item in values) {
                result.Add(item);
            }

            return result;
        }
    }
}