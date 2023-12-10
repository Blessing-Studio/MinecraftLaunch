using MinecraftLaunch;
using MinecraftLaunch.Classes.Interfaces;
using MinecraftLaunch.Components.Authenticator;
using MinecraftLaunch.Components.Checker;
using MinecraftLaunch.Components.Installer;
using MinecraftLaunch.Components.Resolver;
using MinecraftLaunch.Extensions;
using System.Diagnostics;

string gameFolder = "C:\\Users\\w\\Desktop\\temp\\.minecraft";

var _ = (await VanlliaInstaller.EnumerableGameCoreAsync());

var installer = new VanlliaInstaller(gameFolder, "1.12.2", MirrorDownloadManager.Mcbbs);

installer.ProgressChanged += (_, x) => {
    Console.Clear();
    Console.SetCursorPosition(0, 0);
    Console.WriteLine($"{x.Status} - {x.ProgressStatus} - {x.Progress:0.00} - {x.Speed}");
    Console.SetCursorPosition(0, 0);
};

var result = await installer.InstallAsync();

Console.ReadKey();