using System;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading.Tasks.Dataflow;
using MinecraftLaunch.Modules.Downloaders;
using MinecraftLaunch.Modules.Enum;
using MinecraftLaunch.Modules.Interface;
using MinecraftLaunch.Modules.Models.Download;
using MinecraftLaunch.Modules.Models.Install;
using MinecraftLaunch.Modules.Utilities;

namespace MinecraftLaunch.Modules.Installer;

public class ModsPacksInstaller : InstallerBase<InstallerResponse> {
    private int _totalDownloaded;

    private int _needToDownload;

    private int _failedFiles = -1;

    public string ModPacksPath { get; set; }

    public string GamePath { get; set; }

    public string GameId { get; set; }

    public ModsPacksInstaller(string modPacksPath, string gamePath, string gameid = null) {
        ModPacksPath = modPacksPath;
        GamePath = gamePath;
        GameId = gameid;
    }

    public override async ValueTask<InstallerResponse> InstallAsync() {
        InvokeStatusChangedEvent(0.1f, "开始获取整合包信息");

        ModsPacksModel info = await GetModsPacksInfoAsync();
        _needToDownload = info.Files.Count;
        string idpath = Path.Combine(Path.GetFullPath(GamePath), "versions", string.IsNullOrEmpty(GameId) ? info.Name : GameId);
        DirectoryInfo di = new DirectoryInfo(Path.Combine(idpath, "mods"));
        if (!di.Exists) {
            di.Create();
        }
        InvokeStatusChangedEvent(0.4f, "开始解析整合包模组链接");

        TransformManyBlock<IEnumerable<ModsPacksFileModel>, (long, long)> urlBlock = new TransformManyBlock<IEnumerable<ModsPacksFileModel>, (long, long)>(urls => urls.Select(file => (file.ProjectId, file.FileId)));
        using (ZipArchive subPath = ZipFile.OpenRead(ModPacksPath)) {
            foreach (ZipArchiveEntry i in subPath.Entries) {
                if (i.FullName.StartsWith("overrides") && !string.IsNullOrEmpty(ExtendUtil.GetString(subPath.GetEntry(i.FullName)))) {
                    string cutpath = i.FullName.Replace("overrides/", string.Empty);
                    FileInfo v = new FileInfo(Path.Combine(idpath, cutpath));
                    if (!Directory.Exists(Path.Combine(idpath, v.Directory.FullName))) {
                        Directory.CreateDirectory(Path.Combine(idpath, v.Directory.FullName));
                    }

                    ExtendUtil.ExtractTo(subPath.GetEntry(i.FullName), Path.Combine(idpath, cutpath));
                }
            }
        }
        GameCoreUtil.GetGameCore(GamePath, GameId);
        InvokeStatusChangedEvent(0.45f, "开始下载整合包模组");

        ActionBlock<(long, long)> actionBlock = new ActionBlock<(long, long)>(async delegate ((long, long) t) {
            _ = 1;
            try {
                string url = await new CurseForgeUtil(CurseForgeUtil.Key).GetModpackDownloadUrlAsync(t.Item1, t.Item2);
                if ((await HttpUtil.HttpDownloadAsync(new HttpDownloadRequest {
                    Url = url,
                    Directory = di,
                    FileName = Path.GetFileName(url)
                })).HttpStatusCode != HttpStatusCode.OK) {
                    _failedFiles++;
                }
                _totalDownloaded++;
                int e2 = _totalDownloaded / _needToDownload;
                InvokeStatusChangedEvent(0.2f + (float)e2 * 0.8f, $"下载Mod中：{_totalDownloaded}/{_needToDownload}");
            }
            catch (Exception) {
                _failedFiles++;
            }
        }, new ExecutionDataflowBlockOptions {
            BoundedCapacity = 32,
            MaxDegreeOfParallelism = 32
        });
        DataflowLinkOptions linkOptions = new DataflowLinkOptions {
            PropagateCompletion = true
        };
        urlBlock.LinkTo(actionBlock, linkOptions);
        urlBlock.Post(info.Files);
        urlBlock.Complete();
        await actionBlock.Completion;
        InvokeStatusChangedEvent(1f, "安装完成");

        if (_failedFiles != -1) {
            return new InstallerResponse {
                Exception = null,
                GameCore = null,
                Success = false
            };
        }
        return new InstallerResponse {
            Exception = null,
            GameCore = new GameCoreUtil(GamePath).GetGameCore(GameId),
            Success = true
        };
    }

    public async ValueTask<ModsPacksModel> GetModsPacksInfoAsync() {
        string json = string.Empty;
        using ZipArchive zipinfo = ZipFile.OpenRead(ModPacksPath);
        if (zipinfo.GetEntry("manifest.json") != null) {
            json = ExtendUtil.GetString(zipinfo.GetEntry("manifest.json"));
        }
        return await ValueTask.FromResult(json.ToJsonEntity<ModsPacksModel>());
    }

    /// <summary>
    /// 获取整合包类型
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public ModpacksType GetModpackType(string path) {
        using var zipItems = ZipFile.OpenRead(path);

        foreach (var zipItem in zipItems.Entries) {
            var fullName = zipItem.FullName;

            if (fullName.Contains("mcbbs.packmeta")) {
                return ModpacksType.Mcbbs;
            }

            if (fullName.Contains("modrinth.index.json")) {
                return ModpacksType.Modrinth;
            }

            if (fullName.Contains("manifest.json")) {
                return ModpacksType.Curseforge;
            }
        }

        return ModpacksType.Unknown;
    }

    /// <summary>
    /// Mcbbs
    /// </summary>
    /// <param name="path"></param>
    public void McbbsModpacksInstall(string path) {
        using ZipArchive zipinfo = ZipFile.OpenRead(path);
        var jsonEntry = zipinfo.GetEntry("manifest.json");
        if (jsonEntry == null) {
            return;
        }

        string json = ExtendUtil.GetString(jsonEntry);
        var modpackInfo = json.ToJsonEntity<ModsPacksModel>();
        var gamcorePath = GameCoreUtil.GetGameCore(GamePath, modpackInfo.Name)
            .GetGameCorePath();

        foreach (ZipArchiveEntry i in zipinfo.Entries.AsParallel()) {
            if (!i.FullName.StartsWith("overrides")) {
                continue;
            }

            string cutpath = i.FullName.Replace("overrides/", string.Empty);
            string fullPath = Path.Combine(gamcorePath, cutpath);
            FileInfo v = new(fullPath);

            if (!v.Directory!.Exists) {
                v.Directory.Create();
            }

            i.ExtractToFile(fullPath, overwrite: true);
        }
    }

    /// <summary>
    /// Modrinth
    /// </summary>
    /// <param name="path"></param>
    public async void ModrinthModpacksInstall(string path) {
        using var zipItems = ZipFile.OpenRead(path);
        var jsonEntry = zipItems.GetEntry("modrinth.index.json");
        if (jsonEntry == null) return;

        string json = ExtendUtil.GetString(jsonEntry);
        var modpackInfo = json.ToJsonEntity<ModrinthJsonModel>();
        float _totalDownloaded = 0, _needToDownload = modpackInfo.Files.Count();

        var gamecorePath = GameCoreUtil.GetGameCore(GamePath, modpackInfo.Name).GetGameCorePath();
        var actionBlock = new ActionBlock<IEnumerable<Files>>(async x => {
            try {
                foreach (var item in x.AsParallel()) {
                    foreach (var url in item.Downloads.AsParallel()) {
                        var folder = item.Path?.Split('/').FirstOrDefault()!;

                        await FileDownloader.DownloadAsync(new DownloadRequest {
                            Url = url,
                            FileName = Path.GetFileName(url),
                            Directory = new(Path.Combine(gamecorePath, folder))
                        });

                        _totalDownloaded++;
                        var progress = (_totalDownloaded / _needToDownload * 0.8f) * 100;
                    }
                }
            }
            catch (Exception) { 
            }
        }, new ExecutionDataflowBlockOptions {
            BoundedCapacity = 64,
            MaxDegreeOfParallelism = 64
        });

        actionBlock.Post(modpackInfo.Files);
        actionBlock.Complete();
        await actionBlock.Completion;

        foreach (ZipArchiveEntry i in zipItems.Entries.AsParallel()) {
            if (!i.FullName.StartsWith("overrides")) continue;

            string cutpath = i.FullName.Replace("overrides/", string.Empty);
            string fullPath = Path.Combine(gamecorePath, cutpath);
            FileInfo v = new FileInfo(fullPath);

            if (!v.Directory.Exists) {
                v.Directory.Create();
            }

            i.ExtractToFile(fullPath, overwrite: true);
        }
    }
}