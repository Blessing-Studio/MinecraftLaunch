using MinecraftLaunch;
using MinecraftLaunch.Components.Resolver;
using MinecraftLaunch.Components.Installer;

GameResolver gameResolver = new("C:\\Users\\w\\Downloads\\.minecraft");

VanlliaInstaller vanlliaInstaller = new(gameResolver, "1.12.2");
vanlliaInstaller.ProgressChanged += (_, args) => {
    Console.WriteLine($"{args.Progress * 100:0.00} - {args.Status} - {args.ProgressStatus}");
};

await vanlliaInstaller.InstallAsync();

Console.WriteLine();
Console.WriteLine();
Console.WriteLine();
Console.WriteLine();
Console.WriteLine();

ForgeInstaller forgeInstaller = new(gameResolver.GetGameEntity("1.12.2"),
    (await ForgeInstaller.EnumerableFromVersionAsync("1.12.2")).First(),
    "C:\\Program Files\\Java\\jdk1.8.0_301\\bin\\javaw.exe",
    "1.12.2-forge-114514");

forgeInstaller.ProgressChanged += (_, args) => {
    Console.WriteLine($"{args.Progress * 100:0.00} - {args.Status} - {args.ProgressStatus}");
};

await forgeInstaller.InstallAsync();
Console.ReadKey();