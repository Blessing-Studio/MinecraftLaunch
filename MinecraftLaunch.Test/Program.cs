using MinecraftLaunch.Components.Launcher;
using MinecraftLaunch.Components.Resolver;
using MinecraftLaunch.Classes.Models.Launch;
using MinecraftLaunch.Components.Authenticator;
using System.IO.Compression;
using MinecraftLaunch.Classes.Models.Download;
using MinecraftLaunch.Components.Downloader;
using MinecraftLaunch.Extensions;


var list = new List<DownloadRequest> {
    new() {
        Url = "https://download.mcbbs.net/version/1.20.4/client",
        Path = "C:\\Users\\w\\Desktop\\temp\\cache",
        Name = "1.jar"
    },
    new() {
        Url = "https://download.mcbbs.net/version/1.20.3/client",
        Path = "C:\\Users\\w\\Desktop\\temp\\cache",
        Name = "2.jar"
    },
    new() {
        Url = "https://download.mcbbs.net/version/1.20.2/client",
        Path = "C:\\Users\\w\\Desktop\\temp\\cache",
        Name = "3.jar"
    },
    new() {
        Url = "https://download.mcbbs.net/version/1.20.1/client",
        Path = "C:\\Users\\w\\Desktop\\temp\\cache",
        Name = "4.jar"
    },
    new() {
        Url = "https://download.mcbbs.net/version/1.16.5/client",
        Path = "C:\\Users\\w\\Desktop\\temp\\cache",
        Name = "5.jar"
    },
    new() {
        Url = "https://download.mcbbs.net/version/1.7.10/client",
        Path = "C:\\Users\\w\\Desktop\\temp\\cache",
        Name = "6.jar"
    },
    new() {
        Url = "https://download.mcbbs.net/version/1.12.2/client",
        Path = "C:\\Users\\w\\Desktop\\temp\\cache",
        Name = "7.jar"
    },
};

BatchDownloader downloader = new();
downloader.Setup(list);

downloader.ProgressChanged += (_, args) => {
    Console.WriteLine($"{args.ToSpeedText()} - {args.ToPercentage() * 100:0.00}%");
};

var result = await downloader.DownloadAsync();
Console.WriteLine(result);
return;
var account = new OfflineAuthenticator("Yang114").Authenticate();
var resolver = new GameResolver(".minecraft");

var config = new LaunchConfig {
    Account = account,
    IsEnableIndependencyCore = true,
    JvmConfig = new(@"C:\Program Files\Java\jdk1.8.0_301\bin\javaw.exe") {
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