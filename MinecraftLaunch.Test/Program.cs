using MinecraftLaunch;
using MinecraftLaunch.Components.Resolver;
using MinecraftLaunch.Components.Installer;
using MinecraftLaunch.Components.Watcher;
using MinecraftLaunch.Classes.Models.Event;

# region ServerPing 

ServerPingWatcher serverPingWatcher = new(25565, "mc.163mc.cn", 47);

serverPingWatcher.ServerConnectionProgressChanged += OnServerConnectionProgressChanged;

serverPingWatcher.ServerLatencyChanged += (_, args) => {
    Console.WriteLine($"{args.Latency}ms");
};

await serverPingWatcher.StartAsync();

void OnServerConnectionProgressChanged(object? sender, ProgressChangedEventArgs args) {
    Console.WriteLine($"{args.Progress * 100:0.00} - {args.Status} - {args.ProgressStatus}");
    if (args.Status == TaskStatus.Canceled) {
        serverPingWatcher.ServerConnectionProgressChanged -= OnServerConnectionProgressChanged;
    }
}

#endregion

# region Forge Install

//GameResolver gameResolver = new("C:\\Users\\w\\Downloads\\.minecraft");

//VanlliaInstaller vanlliaInstaller = new(gameResolver, "1.12.2");
//vanlliaInstaller.ProgressChanged += (_, args) => {
//    Console.WriteLine($"{args.Progress * 100:0.00} - {args.Status} - {args.ProgressStatus}");
//};

//await vanlliaInstaller.InstallAsync();

//Console.WriteLine();
//Console.WriteLine();
//Console.WriteLine();
//Console.WriteLine();
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

Console.ReadKey();