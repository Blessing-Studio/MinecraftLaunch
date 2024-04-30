using MinecraftLaunch.Components.Resolver;
using MinecraftLaunch.Components.Installer;
using MinecraftLaunch;

MirrorDownloadManager.IsUseMirrorDownloadSource = true;

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

Console.ReadKey();