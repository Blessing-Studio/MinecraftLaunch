using MinecraftLaunch;
using MinecraftLaunch.Components.Fetcher;
using MinecraftLaunch.Components.Installer;
using MinecraftLaunch.Components.Resolver;
using MinecraftLaunch.Extensions;

string gameFolder = "C:\\Users\\w\\Desktop\\temp\\.minecraft";
var installer = new VanlliaInstaller(new GameResolver(gameFolder), "1.16.5", MirrorDownloadManager.Mcbbs);

installer.ProgressChanged += (_, x) => {
    Console.Clear();
    Console.SetCursorPosition(0, 0);
    Console.WriteLine($"{x.Status} - {x.ProgressStatus} - {x.Progress.ToPercentage(0.0d, 0.65d) * 100:F2}%");
    Console.SetCursorPosition(0, 0);
};

var result = await installer.InstallAsync();

var fInstaller = new ForgeInstaller(new GameResolver(gameFolder).GetGameEntity("1.16.5"),
    (await ForgeInstaller.EnumerableFromVersionAsync("1.16.5")).FirstOrDefault(),new JavaFetcher().Fetch().First().JavaPath);

fInstaller.ProgressChanged += (_, x) => {
    Console.Clear();
    Console.SetCursorPosition(0, 0);
    Console.WriteLine($"{x.Status} - {x.ProgressStatus} - {x.Progress.ToPercentage(0.65d, 1.0d) * 100:F2}%");
    Console.SetCursorPosition(0, 0);
};

await fInstaller.InstallAsync();

Console.ReadKey();

