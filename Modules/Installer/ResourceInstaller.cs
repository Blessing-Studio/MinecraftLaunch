using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using MinecraftLaunch.Modules.Interface;
using MinecraftLaunch.Modules.Models.Download;
using MinecraftLaunch.Modules.Models.Install;
using MinecraftLaunch.Modules.Models.Launch;
using MinecraftLaunch.Modules.Parser;
using MinecraftLaunch.Modules.Utils;
using Natsurainko.Toolkits.IO;
using Natsurainko.Toolkits.Network;
using Natsurainko.Toolkits.Network.Model;
using static System.Net.Mime.MediaTypeNames;

namespace MinecraftLaunch.Modules.Installer;

public class ResourceInstaller {
    public GameCore GameCore { get; set; }

    public List<IResource> FailedResources { get; set; } = new List<IResource>();

    public static int MaxDownloadThreads { get; set; } = 64;

    public async ValueTask<ResourceInstallResponse> DownloadAsync(Action<string, float> func) {
        var progress = new Progress<(string, float)>();
        var root = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        void Progress_ProgressChanged(object _, (string, float) e) => func(e.Item1, e.Item2);

        progress.ProgressChanged += Progress_ProgressChanged!;

        var clientFile = GetFileResources()?.FirstOrDefault();
        if (clientFile != null && !clientFile.ToFileInfo().Exists) {
            var request = clientFile.ToDownloadRequest();
            if (!request.Directory.Exists) {
                request.Directory.Create();
            }

            if (APIManager.Current != APIManager.Mojang) {
                request.Url = $"{APIManager.Current.Host}/version/{Path.GetFileNameWithoutExtension(clientFile.Name)}/client";
            }

            var httpDownloadResponse = await HttpWrapper.HttpDownloadAsync(request);
            if (httpDownloadResponse.HttpStatusCode != HttpStatusCode.OK) {
                FailedResources.Add(clientFile);
            }
        }

        var manyBlock = new TransformManyBlock<List<IResource>, IResource>(x => x.Where(x => {
            if (string.IsNullOrEmpty(x.CheckSum) && x.Size == 0) {
                return false;
            }

            if (x.ToFileInfo().Verify(x.CheckSum) && x.ToFileInfo().Verify(x.Size)) {
                return true;
            }

            return true;
        }));

        int post = 0, output = 0;

        var resources = new List<IResource>();
        resources.AddRange(GameCore.LibraryResources!.Where(x => x.IsEnable && !x.ToFileInfo().Exists).Select(x => (IResource)x).ToList());
        resources.AddRange((await this.GetAssetResourcesAsync()).Where(x => !x.ToFileInfo().Exists).ToList());

        if (resources.Count > 0) {
            var actionBlock = new ActionBlock<IResource>(async resource => {
                post++;
                var request = resource.ToDownloadRequest();

                if (!request.Directory.Exists) {
                    request.Directory.Create();
                }

                try {
                    var info = request.Directory.FullName.Substring(request.Directory.FullName.IndexOf(".minecraft"));
                    var text = Path.Combine(root, info, request.FileName);
                    //先尝试使用缓存，不行就下一遍
                    if (File.Exists(text) && !resource.ToFileInfo().Exists) {
                        File.Copy(text, Path.Combine(request.Directory.FullName, request.FileName), true);
                    } else if (!resource.ToFileInfo().Exists)//缓存和实际目录都没有此依赖的情况
                      {
                        var httpDownloadResponse = await HttpUtil.HttpDownloadAsync(request);

                        if (httpDownloadResponse.HttpStatusCode != HttpStatusCode.OK)
                            this.FailedResources.Add(resource);
                        else {
                            //将缓存没有的资源复制到缓存里，以供下次使用
                            if (!Directory.Exists(Path.Combine(root, info))) {
                                Directory.CreateDirectory(Path.Combine(root, info));
                            }

                            httpDownloadResponse.FileInfo.CopyTo(text, true);
                        }
                    }
                }
                catch {
                    this.FailedResources.Add(resource);
                }

                output++;

                ((IProgress<(string, float)>)progress).Report(($"{output}/{post}", output / (float)post));
            }, new ExecutionDataflowBlockOptions {
                BoundedCapacity = MaxDownloadThreads,
                MaxDegreeOfParallelism = MaxDownloadThreads
            });
            var disposable = manyBlock.LinkTo(actionBlock, new DataflowLinkOptions { PropagateCompletion = true });

            manyBlock.Post(resources);
            manyBlock.Complete();

            await actionBlock.Completion;
            disposable.Dispose();

            GC.Collect();
        }

        progress.ProgressChanged -= Progress_ProgressChanged!;

        return new ResourceInstallResponse {
            FailedResources = this.FailedResources,
            SuccessCount = post - this.FailedResources.Count,
            Total = post
        };
    }

    public IEnumerable<IResource> GetFileResources() {
        if (GameCore.ClientFile != null)
            yield return GameCore.ClientFile;
    }

    public async ValueTask<List<IResource>> GetAssetResourcesAsync() {
        if (!(GameCore.AssetIndexFile!.FileInfo.Verify(GameCore.AssetIndexFile.Size) || GameCore.AssetIndexFile.FileInfo.Verify(GameCore.AssetIndexFile.CheckSum))) {

            var request = this.GameCore.AssetIndexFile.ToDownloadRequest();

            if (!request.Directory.Exists)
                request.Directory.Create();

            var res = await HttpWrapper.HttpDownloadAsync(request);
        }

        var entity = new AssetJsonEntity();
        entity = entity.FromJson(await File.ReadAllTextAsync(this.GameCore.AssetIndexFile?.ToFileInfo()!.FullName!));

        return new AssetParser(entity, this.GameCore.Root!).GetAssets().Select(x => (IResource)x).ToList();
    }

    public async static ValueTask<List<IResource>> GetAssetFilesAsync(GameCore core) {
        var root = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var asset = new AssetParser(new AssetJsonEntity().FromJson(await File.ReadAllTextAsync(core.AssetIndexFile.ToFileInfo().FullName)), core.Root).GetAssets().Select((Func<AssetResource, IResource>)((AssetResource x) => x)).ToList();
        var res = core.LibraryResources.Where((LibraryResource x) => x.IsEnable).Select((Func<LibraryResource, IResource>)((LibraryResource x) => x)).ToList();
        res.AddRange(asset);
        res.Sort((x, x1) => x.Size.CompareTo(x1.Size));

        foreach (var i in asset) {
            if (File.Exists(Path.Combine(Path.Combine(root, i.ToDownloadRequest().Directory.FullName.Substring(i.ToDownloadRequest().Directory.FullName.IndexOf(".minecraft"))), i.ToDownloadRequest().FileName))) {
                Console.WriteLine("文件 {0} 存在在官方目录！", i.ToDownloadRequest().FileName);
            }
        }

        return res;
    }

    [Obsolete]
    public IEnumerable<IResource> GetModLoaderResourcesAsync() {
        var entity = new GameCoreJsonEntity().FromJson(File.ReadAllText(Path.Combine(this.GameCore.Root.FullName, "versions", this.GameCore.Id, $"{this.GameCore.Id}.json")));
        List<LibraryResource> list = entity.Libraries.Select(x => {
            return new LibraryResource() {
                Root = GameCore.Root,
                Name = x.Name,
                Size = x.Name.Length,
                CheckSum = "114514",
                Url = x.Url,
            };
        }).ToList();

        foreach (var i in list) {
            if (!i.ToFileInfo().Exists) {
                yield return i;
            }
        }
    }

    public ResourceInstaller(GameCore core) {
        GameCore = core;
    }
}
