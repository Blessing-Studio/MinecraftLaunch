using MinecraftLaunch;
using MinecraftLaunch.Base.Enums;
using MinecraftLaunch.Extensions;
using MinecraftLaunch.Base.Models.Auth;
using MinecraftLaunch.Base.Models.Event;
using MinecraftLaunch.Base.Models.Launch;
using MinecraftLaunch.Components.Checker;
using MinecraftLaunch.Components.Resolver;
using MinecraftLaunch.Components.Launcher;

string mPath = "";
TomlResolver resolver = new(File.ReadAllText("C:\\Users\\w\\Desktop\\mods.toml"));
Console.WriteLine(resolver["authors"]);

return;
//DefaultGameLocator locator = new(path);
//locator.GetGames(out var eg).ToList().ForEach(game =>Console.WriteLine(game.Name));
//Console.WriteLine(eg.Count);
GameResolver gr = new(mPath);

Console.ReadKey();

Console.WriteLine("开始检查资源文件");
var coreEntry = gr.GetGameEntity("1.12.2-Forge_14.23.5.2860-OptiFine_HD_U_C5");
ResourceChecker checker = new(coreEntry);
var value = await checker.CheckAsync();

if (!value) {
    Console.WriteLine($"开始补全文件，共计 {checker.MissingResources!.Count} 个文件需要下载");
    await Task.Delay(1000);

    await checker.MissingResources.DownloadResourceEntrysAsync(MirrorDownloadManager.Mcbbs,
        args => {
            Console.Clear();
            Console.SetCursorPosition(0, 0);
            Console.WriteLine($"总下载进度：{args.ToPercentage()}% - 文件个数：{args.CompletedCount} / {args.TotalCount} items - 下载速度：{args.ToSpeedText()}");
            Console.SetCursorPosition(0, 0);
        });
}

Console.WriteLine("开始启动游戏");
Launcher launcher = new(new GameResolver(mPath), new(new OfflineAccount() {
    Name = "666",
    Type = AccountType.Offline,
    Uuid = Guid.NewGuid(),
    AccessToken = Guid.NewGuid().ToString("N"),
}) {
    JvmConfig = new JvmConfig("C:\\Program Files\\Java\\jdk1.8.0_301\\bin\\javaw.exe"),
    IsEnableIndependencyCore = false
});

var watcher = await launcher.LaunchAsync("1.12.2-Forge_14.23.5.2860-OptiFine_HD_U_C5");
watcher.Exited += OnExited;
watcher.OutputLogReceived += OnOutputLogReceived;

void OnExited(object? sender, ExitedEventArgs e) {
    Console.WriteLine($"Done! [{e.ExitCode}]");
}

void OnOutputLogReceived(object? sender, LogReceivedEventArgs e) {
    Console.WriteLine(e.Text);
}

Console.WriteLine("游戏启动完成");
Console.ReadKey();