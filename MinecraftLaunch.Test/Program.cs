using MinecraftLaunch.Components.Launcher;
using MinecraftLaunch.Components.Resolver;
using MinecraftLaunch.Classes.Models.Launch;
using MinecraftLaunch.Components.Authenticator;
using System.IO.Compression;

//new() {
//    "https://download.mcbbs.net/version/1.7.10/client",
//    "https://download.mcbbs.net/version/1.12.2/client",
//    "https://download.mcbbs.net/version/1.16.5/client",
//    "https://download.mcbbs.net/version/1.20.1/client",
//    "https://download.mcbbs.net/version/1.20.2/client",
//    "https://download.mcbbs.net/version/1.20.3/client",
//    "https://download.mcbbs.net/version/1.20.4/client",
//}

var file = new DownloadFile() { 
    DownloadPath = "C:\\Users\\w\\Desktop\\temp\\natives",
    FileName = "test.jar",
    DownloadUri = "https://download.mcbbs.net/version/1.20.4/client",
};

file.Changed += (_, x) => {
    Console.WriteLine($"{x.ProgressPercentage * 100:0.00} - {x.Speed}");
};

await DownloadHelper.AdvancedDownloadFile(file,DownloadSettings.Default);

Console.ReadKey();
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