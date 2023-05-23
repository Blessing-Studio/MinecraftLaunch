using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using MinecraftLaunch.Modules.Interface;
using MinecraftLaunch.Modules.Models.Download;
using MinecraftLaunch.Modules.Models.Install;
using MinecraftLaunch.Modules.Toolkits;
using Natsurainko.Toolkits.Network;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MinecraftLaunch.Modules.Installer
{
    public class QuiltInstaller : InstallerBase<InstallerResponse>
    {
        public QuiltInstallBuild QuiltBuild { get; private set; }

        public GameCoreToolkit GameCoreLocator { get; private set; }

        public string CustomId { get; private set; }

        public static void ParseBuild(QuiltInstallBuild QuiltBuild)
        {
            List<QuiltLibraryJsonEntity> libraries = QuiltBuild.LauncherMeta.Libraries["common"];
            if (QuiltBuild.LauncherMeta.Libraries["common"] != null)
            {
                libraries.AddRange(QuiltBuild.LauncherMeta.Libraries["client"]);
            }
            libraries.Insert(0, new QuiltLibraryJsonEntity
            {
                Name = QuiltBuild.Intermediary.Maven
            });
            libraries.Insert(0, new QuiltLibraryJsonEntity
            {
                Name = QuiltBuild.Loader.Maven
            });
            if ((int)QuiltBuild.LauncherMeta.MainClass.Type != 1)
            {
                if (!string.IsNullOrEmpty(((object)QuiltBuild.LauncherMeta.MainClass).ToString()))
                {
                    ((object)QuiltBuild.LauncherMeta.MainClass).ToString();
                }
            }
            else
            {
                _ = QuiltBuild.LauncherMeta.MainClass.ToObject<Dictionary<string, string>>()["client"];
            }
            _ = QuiltBuild.Intermediary.Version;
            libraries.ForEach(async delegate (QuiltLibraryJsonEntity x)
            {
                if (x.Name.Contains("fabricmc") || x.Name.Contains("ow2.asm"))
                {
                    x.Url = UrlExtension.Combine(new string[2]
                    {
                    "https://maven.fabricmc.net",
                    UrlExtension.Combine(LibraryResource.FormatName(x.Name).ToArray())
                    });
                }
                else
                {
                    x.Url = UrlExtension.Combine(new string[2]
                    {
                    "https://maven.quiltmc.org/repository/release",
                    UrlExtension.Combine(LibraryResource.FormatName(x.Name).ToArray())
                    });
                }
            });
            List<LibraryResource> res = libraries.Select((QuiltLibraryJsonEntity y) => new LibraryResource
            {
                Root = new DirectoryInfo(Path.GetFullPath(".minecraft")),
                Name = y.Name,
                Url = y.Url
            }).ToList();
            int count = 0;
            res.ForEach(async delegate (LibraryResource x)
            {
                await HttpWrapper.HttpDownloadAsync(x.Url, x.ToDownloadRequest().Directory.FullName, (string)null);
                count++;
                Console.WriteLine($"下载依赖文件中：{count}/{res.Count}");
            });
        }

        public override async ValueTask<InstallerResponse> InstallAsync()
        {
            int count = 0;
            int post = 0;
            List<LibraryResource> files = new List<LibraryResource>();
            InvokeStatusChangedEvent(0.25f, "开始分析生成");

            List<QuiltLibraryJsonEntity> libraries = QuiltBuild.LauncherMeta.Libraries["common"];
            if (QuiltBuild.LauncherMeta.Libraries["common"] != null)
            {
                libraries.AddRange(QuiltBuild.LauncherMeta.Libraries["client"]);
            }

            libraries.Insert(0, new QuiltLibraryJsonEntity
            {
                Name = QuiltBuild.Intermediary.Maven
            });

            libraries.Insert(0, new QuiltLibraryJsonEntity
            {
                Name = QuiltBuild.Loader.Maven
            });

            string mainClass = (((int)QuiltBuild.LauncherMeta.MainClass.Type == 1) ? QuiltBuild.LauncherMeta.MainClass.ToObject<Dictionary<string, string>>()!["client"] : (string.IsNullOrEmpty(((object)QuiltBuild.LauncherMeta.MainClass).ToString()) ? "net.minecraft.client.main.Main" : ((object)QuiltBuild.LauncherMeta.MainClass).ToString()))!;
            string inheritsFrom = QuiltBuild.Intermediary.Version;

            if (mainClass == "net.minecraft.client.main.Main")
            {
                return new InstallerResponse
                {
                    Success = false,
                    GameCore = null!,
                    Exception = new ArgumentNullException("MainClass")
                };
            }
            InvokeStatusChangedEvent(0.45f, "开始下载依赖文件");

            foreach (var x in libraries.AsParallel())
            {
                if (x.Name.Contains("fabricmc") || x.Name.Contains("ow2.asm"))
                {
                    x.Url = UrlExtension.Combine(new string[2] {
                    "https://maven.fabricmc.net",
                    UrlExtension.Combine(LibraryResource.FormatName(x.Name).ToArray())
                });
                }
                else
                {
                    x.Url = UrlExtension.Combine(new string[2] {
                    "https://maven.quiltmc.org/repository/release",
                    UrlExtension.Combine(LibraryResource.FormatName(x.Name).ToArray())
                });
                }
                files.Add(new LibraryResource
                {
                    Root = new DirectoryInfo(Path.GetFullPath(GameCoreLocator.Root.FullName)),
                    Name = x.Name,
                    Url = x.Url
                });
            }

            TransformManyBlock<List<LibraryResource>, LibraryResource> manyBlock = new TransformManyBlock<List<LibraryResource>, LibraryResource>((List<LibraryResource> x) => x.Where((LibraryResource x) => true));
            ActionBlock<LibraryResource> actionBlock = new ActionBlock<LibraryResource>(async delegate (LibraryResource resource)
            {
                post++;
                if ((await HttpWrapper.HttpDownloadAsync(resource.Url, resource.ToFileInfo().Directory!.FullName, (string)null!)).HttpStatusCode != HttpStatusCode.OK)
                {
                    Console.WriteLine(resource.Url);
                    InvokeStatusChangedEvent((float)count / (float)post, "依赖文件：" + resource.ToFileInfo().Name + " 下载失败");
                }
                count++;
                InvokeStatusChangedEvent((float)count / (float)post, $"下载依赖文件中 {count}/{post}");
            }, new ExecutionDataflowBlockOptions
            {
                BoundedCapacity = 128,
                MaxDegreeOfParallelism = 128
            });

            IDisposable disposable = manyBlock.LinkTo(actionBlock, new DataflowLinkOptions
            {
                PropagateCompletion = true
            });

            manyBlock.Post(files);
            manyBlock.Complete();
            await actionBlock.Completion;
            disposable.Dispose();

            InvokeStatusChangedEvent(0.55f, "开始检查继承的核心");
            if (GameCoreLocator.GetGameCore(QuiltBuild.Intermediary.Version) == null) {           
                var installer = new GameCoreInstaller(GameCoreLocator, QuiltBuild.Intermediary.Version);
                installer.ProgressChanged += (_, e) => {
                    InvokeStatusChangedEvent(0.45f + 0.15000004f * e.Progress, "正在下载继承的游戏核心：" + e.ProgressDescription);
                };

                await installer.InstallAsync();
            }

            InvokeStatusChangedEvent(0.85f, "开始写入文件");
            QuiltGameCoreJsonEntity entity = new QuiltGameCoreJsonEntity
            {
                Id = (string.IsNullOrEmpty(CustomId) ? ("quilt-loader-" + QuiltBuild.Loader.Version + "-" + QuiltBuild.Intermediary.Version) : CustomId),
                InheritsFrom = inheritsFrom,
                ReleaseTime = DateTime.Now.ToString("O"),
                Time = DateTime.Now.ToString("O"),
                Type = "release",
                MainClass = mainClass,
                Arguments = new QuiltArgumentsJsonEntity
                {
                    Game = new List<JToken>()
                },
                Libraries = libraries
            };

            FileInfo versionJsonFile = new(Path.Combine(GameCoreLocator.Root.FullName, "versions", entity.Id, entity.Id + ".json"));
            if (!versionJsonFile.Directory!.Exists)
            {
                versionJsonFile.Directory.Create();
            }

            await File.WriteAllTextAsync(versionJsonFile.FullName, entity.ToJson(IsIndented: true));
            InvokeStatusChangedEvent(1f, "安装完成");

            return new InstallerResponse
            {
                Success = true,
                GameCore = GameCoreLocator.GetGameCore(entity.Id),
                Exception = null!
            };
        }

        public static async ValueTask<string[]> GetSupportedMcVersionsAsync()
        {
            List<string> supportedMcVersions = new List<string>();
            foreach (JToken i in JArray.Parse(await (await HttpWrapper.HttpGetAsync("https://meta.quiltmc.org/v3/versions/game", (Tuple<string, string>)null, HttpCompletionOption.ResponseContentRead)).Content.ReadAsStringAsync()))
            {
                supportedMcVersions.Add(((object)i[(object)"version"]).ToString());
            }
            return supportedMcVersions.ToArray();
        }

        public static async ValueTask<QuiltInstallBuild[]> GetQuiltBuildsByVersionAsync(string mcVersion)
        {
            _ = 1;
            try
            {
                using HttpResponseMessage responseMessage = await HttpWrapper.HttpGetAsync("https://meta.quiltmc.org/v3/versions/loader/" + mcVersion, (Tuple<string, string>)null, HttpCompletionOption.ResponseContentRead);
                responseMessage.EnsureSuccessStatusCode();
                return JsonConvert.DeserializeObject<List<QuiltInstallBuild>>(await responseMessage.Content.ReadAsStringAsync()).ToArray();
            }
            catch
            {
                return Array.Empty<QuiltInstallBuild>();
            }
        }

        public QuiltInstaller(GameCoreToolkit coreLocator, QuiltInstallBuild build, string customId = null)
        {
            QuiltBuild = build;
            GameCoreLocator = coreLocator;
            CustomId = customId;
        }

        public QuiltInstaller(string root, QuiltInstallBuild build, string customId = null)
        {
            QuiltBuild = build;
            GameCoreLocator = new(root);
            CustomId = customId;
        }
    }
}