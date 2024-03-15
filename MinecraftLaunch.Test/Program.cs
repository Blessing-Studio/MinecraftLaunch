using MinecraftLaunch;
using MinecraftLaunch.Components.Launcher;
using MinecraftLaunch.Components.Resolver;
using MinecraftLaunch.Classes.Models.Launch;
using MinecraftLaunch.Components.Authenticator;
using MinecraftLaunch.Components.Fetcher;
using MinecraftLaunch.Components.Installer;
using MinecraftLaunch.Extensions;
using MinecraftLaunch.Components.Checker;
using MinecraftLaunch.Utilities;

try {
    MirrorDownloadManager.IsUseMirrorDownloadSource = true;

    var account = new OfflineAuthenticator("Yang114").Authenticate();
    var resolver = new GameResolver(".minecraft");
    var id = "1.12.2";

    VanlliaInstaller installer = new(resolver, id, MirrorDownloadManager.Bmcl);
    installer.ProgressChanged += (sender, args) => {
        Console.WriteLine($"{args.Progress * 100:0.00}% --- {args.ProgressStatus} --- {args.Status}");
    };

    await installer.InstallAsync();

    var config = new LaunchConfig {
        Account = account,
        IsEnableIndependencyCore = true,
        JvmConfig = new(JavaUtil.GetCurrentJava(new JavaFetcher().Fetch(), resolver.GetGameEntity(id)).JavaPath) {
            MaxMemory = 1024,
        }
    };

    Launcher launcher = new(resolver, config);
    var gameProcessWatcher = await launcher.LaunchAsync(id);

    //获取输出日志
    gameProcessWatcher.OutputLogReceived += (sender, args) => {
        Console.WriteLine(args.Text);
    };

    //检测游戏退出
    gameProcessWatcher.Exited += (sender, args) => {
        Console.WriteLine("exit");
    };
} catch (Exception ex) {
    Console.WriteLine(ex.ToString());
}

Console.ReadKey();