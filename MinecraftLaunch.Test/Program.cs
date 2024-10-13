using MinecraftLaunch;
using MinecraftLaunch.Extensions;
using MinecraftLaunch.Classes.Models.Auth;
using MinecraftLaunch.Classes.Models.Launch;
using MinecraftLaunch.Components.Resolver;
using MinecraftLaunch.Components.Analyzer;
using MinecraftLaunch.Components.Launcher;
using MinecraftLaunch.Components.Installer;
using MinecraftLaunch.Components.Authenticator;
using Flurl.Http;
using System.Buffers;
using System.IO;
using MinecraftLaunch.Classes.Models.Download;
using System.Threading.Tasks.Dataflow;
using MinecraftLaunch.Classes.Interfaces;
using System.Timers;
using MinecraftLaunch.Classes.Models.Event;
using MinecraftLaunch.Components.Checker;
using System.Net;
using DownloadProgressChangedEventArgs = MinecraftLaunch.Classes.Models.Event.DownloadProgressChangedEventArgs;


//RD rd = new();
//rd.Completed += OnCompleted;
//rd.ProgressChanged += OnProgressChanged;

GameResolver gameResolver = new("C:\\Users\\wxysd\\Desktop\\temp\\.minecraft");
//ResourceChecker resourceChecker = new(gameResolver.GetGameEntity("1.12.2"));
//await resourceChecker.CheckAsync();

VanlliaInstaller vanlliaInstaller = new(gameResolver, "1.12.2");
vanlliaInstaller.ProgressChanged += VanlliaInstaller_ProgressChanged;

await vanlliaInstaller.InstallAsync();

void VanlliaInstaller_ProgressChanged(object? sender, ProgressChangedEventArgs e) {
    Console.WriteLine($"{e.Progress:P2} - {e.ProgressStatus}");
}

//await rd.DownloadAsync(resourceChecker.MissingResources);

//void OnCompleted(object? sender, EventArgs e) {
//    Console.WriteLine("Completed!");
//}

//void OnProgressChanged(object? sender, DownloadProgressChangedEventArgs e) {
//    Console.WriteLine($"Progress:{(e.DownloadedBytes / e.TotalBytes) * 100:0.00}% - {e.CompletedCount}/{e.TotalCount} - Speed:{GetSpeedText(e.Speed)} - FailedCount:{e.FailedCount}");
//}

//Console.ReadKey();
return;

#region 

MirrorDownloadManager.IsUseMirrorDownloadSource = false;

try {
    #region ServerPing 

    //ServerPingWatcher serverPingWatcher = new(25565, "mc.163mc.cn", 47);

    //serverPingWatcher.ServerConnectionProgressChanged += OnServerConnectionProgressChanged;

    //serverPingWatcher.ServerLatencyChanged += (_, args) => {
    //    Console.WriteLine($"{args.Latency}ms");
    //};

    //await serverPingWatcher.StartAsync();

    //void OnServerConnectionProgressChanged(object? sender, ProgressChangedEventArgs args) {
    //    Console.WriteLine($"{args.Progress * 100:0.00} - {args.Status} - {args.ProgressStatus}");
    //    if (args.Status == TaskStatus.Canceled) {
    //        serverPingWatcher.ServerConnectionProgressChanged -= OnServerConnectionProgressChanged;
    //    }
    //}

    #endregion

    #region Forge Install

    //GameResolver gameResolver = new("C:\\Users\\w\\Downloads\\.minecraft");

    //VanlliaInstaller vanlliaInstaller = new(gameResolver, "1.12.2");
    //vanlliaInstaller.ProgressChanged += (_, args) => {
    //    Console.WriteLine($"{args.Progress * 100:0.00} - {args.Status} - {args.ProgressStatus}");
    //};

    //await vanlliaInstaller.InstallAsync();

    //Console.WriteLine();

    //ForgeInstaller forgeInstaller = new(gameResolver.GetGameEntity("1.12.2"),
    //    (await ForgeInstaller.EnumerableFromVersionAsync("1.12.2")).First(),
    //    "C:\\Program Files\\Java\\jdk1.8.0_301\\bin\\javaw.exe",
    //    "1.12.2-forge-114514");

    //forgeInstaller.ProgressChanged += (_, args) => {
    //    Console.WriteLine($"{args.Progress * 100:0.00} - {args.Status} - {args.ProgressStatus}");
    //};

    //await forgeInstaller.InstallAsync();

    #endregion

    #region Fabric Install

    //GameResolver gameResolver = new("C:\\Users\\w\\Downloads\\.minecraft");
    //VanlliaInstaller vanlliaInstaller = new(gameResolver, "1.16.5");
    //vanlliaInstaller.ProgressChanged += (_, args) => {
    //    Console.WriteLine($"{args.Progress * 100:0.00}% - {args.Status} - {args.ProgressStatus}");
    //};

    //await vanlliaInstaller.InstallAsync();

    //FabricInstaller fabricInstaller = new(gameResolver.GetGameEntity("1.16.5"), 
    //    (await FabricInstaller.EnumerableFromVersionAsync("1.16.5")).First(),
    //    "1.16.5-fabric-114514");

    //fabricInstaller.ProgressChanged += (_, args) => {
    //    Console.WriteLine($"{args.Progress * 100:0.00}% - {args.Status} - {args.ProgressStatus}");
    //};

    //await fabricInstaller.InstallAsync();

    //foreach (var item in gameResolver.GetGameEntitys()){
    //    Console.WriteLine(item.Id);
    //};

    #endregion

    #region Optifine Install

    //GameResolver gameResolver = new("C:\\Users\\w\\Downloads\\.minecraft");

    //VanlliaInstaller vanlliaInstaller = new(gameResolver, "1.12.2", MirrorDownloadManager.Bmcl);
    //vanlliaInstaller.ProgressChanged += (_, args) => {
    //    Console.WriteLine($"{args.Progress * 100:0.00} - {args.Status} - {args.ProgressStatus}");
    //};

    //await vanlliaInstaller.InstallAsync();

    //Console.WriteLine();

    //OptifineInstaller optifineInstaller = new(gameResolver.GetGameEntity("1.12.2"),
    //    (await OptifineInstaller.EnumerableFromVersionAsync("1.12.2")).First(),
    //    "C:\\Program Files\\Java\\jdk1.8.0_301\\bin\\javaw.exe",
    //    "1.12.2-optifine-114514");

    //optifineInstaller.ProgressChanged += (_, args) => {
    //    Console.WriteLine($"{args.Progress * 100:0.00} - {args.Status} - {args.ProgressStatus}");
    //};

    //await optifineInstaller.InstallAsync();

    #endregion

    #region Composition Install

    //GameResolver gameResolver = new("C:\\Users\\wxysd\\Desktop\\temp\\.minecraft");

    //VanlliaInstaller vanlliaInstaller = new(gameResolver, "1.18.2", MirrorDownloadManager.Bmcl);
    //vanlliaInstaller.ProgressChanged += (_, args) => {
    //    Console.WriteLine($"{args.Progress * 100:0.00} - {args.Status} - {args.ProgressStatus}");
    //};

    //await vanlliaInstaller.InstallAsync();

    //Console.WriteLine();

    //ForgeInstaller forgeInstaller = new(gameResolver.GetGameEntity("1.18.2"),
    //    (await ForgeInstaller.EnumerableFromVersionAsync("1.18.2")).First(),
    //    "C:\\Users\\wxysd\\AppData\\Roaming\\.minecraft\\runtime\\java-runtime-gamma\\bin\\javaw.exe",
    //    "1.18.2-Composition-114514",
    //    MirrorDownloadManager.Bmcl);

    //CompositionInstaller compositionInstaller = new(forgeInstaller,
    //    "1.18.2-Composition-114514",
    //    (await OptifineInstaller.EnumerableFromVersionAsync("1.18.2")).First());

    //compositionInstaller.ProgressChanged += (_, args) => {
    //    Console.WriteLine($"{args.Progress * 100:0.00} - {args.Status} - {args.ProgressStatus}");
    //};

    //await compositionInstaller.InstallAsync();

    #endregion

    #region MicrosoftAuthenticator

    //MicrosoftAuthenticator microsoftAuthenticator = new("Your Client ID");
    //await microsoftAuthenticator.DeviceFlowAuthAsync(x => {
    //    Console.WriteLine(x.UserCode);
    //    Console.WriteLine(x.VerificationUrl);
    //});

    //var account = await microsoftAuthenticator.AuthenticateAsync();

    #endregion

    #region Launch

    //var account = new OfflineAuthenticator("Yang114").Authenticate();
    //var resolver = new GameResolver("C:\\Users\\wxysd\\Desktop\\temp\\.minecraft");

    //var config = new LaunchConfig {
    //    Account = account,
    //    IsEnableIndependencyCore = true,
    //    JvmConfig = new(@"C:\Users\wxysd\AppData\Roaming\.minecraft\runtime\java-runtime-gamma\bin\\javaw.exe") {
    //        MaxMemory = 1024,
    //    }
    //};

    //Launcher launcher = new(resolver, config);
    //var gameProcessWatcher = await launcher.LaunchAsync("1.18.2-Composition-114514");

    ////获取输出日志
    //gameProcessWatcher.OutputLogReceived += (sender, args) => {
    //    Console.WriteLine(args.Original);
    //};

    ////检测游戏退出
    //gameProcessWatcher.Exited += (sender, args) => {
    //    Console.WriteLine("exit");
    //};

    #endregion

    #region Crash Analysis

    //GameResolver gameResolver = new("C:\\Users\\w\\Desktop\\总整包\\MC\\mc启动器\\BakaXL\\.minecraft");

    //var crashAnalyzer = new GameCrashAnalyzer(gameResolver.GetGameEntity("1.18.2-Composition-114514"), true);
    //var reports = crashAnalyzer.AnalysisLogs();

    //foreach (var report in reports) {
    //    Console.WriteLine(report);
    //}

    #endregion
} catch (Exception) { }

#endregion

Console.ReadKey();

static string GetSpeedText(double speed) {
    if (speed < 1024.0) {
        return speed.ToString("0") + " B/s";
    }

    if (speed < 1024.0 * 1024.0) {
        return (speed / 1024.0).ToString("0.0") + " KB/s";
    }

    if (speed < 1024.0 * 1024.0 * 1024.0) {
        return (speed / (1024.0 * 1024.0)).ToString("0.00") + " MB/s";
    }

    return "0";
}

class RD {
    private const int BUFFER_SIZE = 4096;
    private const double UPDATE_INTERVAL = 1.0;

    private readonly MemoryPool<byte> _bufferPool = MemoryPool<byte>.Shared;
    private readonly System.Timers.Timer _timer = new(TimeSpan.FromSeconds(UPDATE_INTERVAL));

    private int _totalCount;
    private int _failedCount;
    private int _completedCount;

    private int _totalBytes;
    private int _downloadedBytes;
    private int _previousDownloadedBytes;

    public event EventHandler<EventArgs> Completed;
    public event EventHandler<DownloadProgressChangedEventArgs> ProgressChanged;

    public async Task DownloadAsync(IEnumerable<IDownloadEntry> downloadEntries, CancellationTokenSource tokenSource = default) {
        _totalCount = downloadEntries.Count();
        _totalBytes = downloadEntries.Sum(item => item.Size);
        _timer.Elapsed += (_, _) => UpdateDownloadProgress();
        _timer.Start();

        var actionBlock = new ActionBlock<IDownloadEntry>(async x => {
            if (!await DownloadFileAsync(x)) {
                Interlocked.Increment(ref _failedCount);
            }

        }, new ExecutionDataflowBlockOptions {
            MaxDegreeOfParallelism = 256,
            CancellationToken = tokenSource is null ? default : tokenSource.Token
        });

        foreach (var downloadEntry in downloadEntries) {
            actionBlock.Post(downloadEntry);
        }

        actionBlock.Complete();
        await actionBlock.Completion;

        _timer.Stop();
        UpdateDownloadProgress();
        Completed?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateDownloadProgress() {
        int diffBytes = _downloadedBytes - _previousDownloadedBytes;
        _previousDownloadedBytes = _downloadedBytes;

        var progress = new DownloadProgressChangedEventArgs {
            TotalCount = _totalCount,
            TotalBytes = _totalBytes,
            FailedCount = _failedCount,
            CompletedCount = _completedCount,
            DownloadedBytes = _downloadedBytes,
            Speed = diffBytes / UPDATE_INTERVAL
        };

        ProgressChanged?.Invoke(this, progress);
    }

    private async Task<bool> DownloadFileAsync(IDownloadEntry entry) {
        int bytesRead;
        var buffer = _bufferPool.Rent(BUFFER_SIZE);

        try {
            using var flurlResponse = await entry.Url.GetAsync();

            if (flurlResponse.StatusCode is 302) {
                entry.Url = flurlResponse.ResponseMessage.Headers.Location!.AbsoluteUri;
                return await DownloadFileAsync(entry);
            }

            if (Path.IsPathRooted(entry.Path)) {
                Directory.CreateDirectory(Path.GetDirectoryName(entry.Path));
            }

            using var responseStream = await flurlResponse.GetStreamAsync();
            using var fileStream = new FileStream(entry.Path, FileMode.Create, FileAccess.Write, FileShare.None);

            if (entry.Size is 0) {
                entry.Size = Convert.ToInt32(flurlResponse.ResponseMessage.Content.Headers.ContentLength);
                Interlocked.Add(ref _totalBytes, entry.Size);
            }

            while ((bytesRead = await responseStream.ReadAsync(buffer.Memory)) > 0) {
                await fileStream.WriteAsync(buffer.Memory[..bytesRead]);
                Interlocked.Add(ref _downloadedBytes, bytesRead);
            }

            Interlocked.Increment(ref _completedCount);
            return true;
        } catch (Exception) {
            return false;
        }
    }
}