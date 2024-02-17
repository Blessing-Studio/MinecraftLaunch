using MinecraftLaunch;
using MinecraftLaunch.Components.Launcher;
using MinecraftLaunch.Components.Resolver;
using MinecraftLaunch.Classes.Models.Launch;
using MinecraftLaunch.Components.Authenticator;
using MinecraftLaunch.Components.Fetcher;
using MinecraftLaunch.Components.Installer;
using MinecraftLaunch.Extensions;

MirrorDownloadManager.IsUseMirrorDownloadSource = true;

var account = new OfflineAuthenticator("Yang114").Authenticate();
Directory.CreateDirectory(".minecraft");
var resolver = new GameResolver(".minecraft");

var installer = new VanlliaInstaller(resolver, "1.12.2");
installer.ProgressChanged += (_, args) => {
    Console.WriteLine($"{args.ProgressStatus} - {args.Progress * 100:0.00}%");
};

installer.Completed += (_, _) => {
    Console.WriteLine("Completed!");
};

await installer.InstallAsync();

var config = new LaunchConfig {
    Account = account,
    IsEnableIndependencyCore = true,
    JvmConfig = new(@"C:\Program Files\Java\jdk-17\bin\javaw.exe") {
        MaxMemory = 1024,
    }
};

Launcher launcher = new(resolver, config);
var gameProcessWatcher = await launcher.LaunchAsync("1.12.2");

//获取输出日志
gameProcessWatcher.OutputLogReceived += (sender, args) => {
    Console.WriteLine(args.Text);
};

//检测游戏退出
gameProcessWatcher.Exited += (sender, args) => {
    Console.WriteLine("exit");  
};

Console.ReadKey();