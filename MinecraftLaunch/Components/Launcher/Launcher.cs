using System.Diagnostics;
using MinecraftLaunch.Utilities;
using MinecraftLaunch.Extensions;
using MinecraftLaunch.Classes.Interfaces;
using MinecraftLaunch.Components.Watcher;
using MinecraftLaunch.Components.Resolver;
using MinecraftLaunch.Classes.Models.Game;
using MinecraftLaunch.Classes.Models.Launch;

namespace MinecraftLaunch.Components.Launcher;

/// <summary>
/// 标准 Java版 Minecraft 启动器
/// </summary>
public sealed class Launcher(IGameResolver resolver, LaunchConfig config) : ILauncher {
    private ArgumentsBuilder _argumentsBuilder;

    public LaunchConfig LaunchConfig => config;

    public IGameResolver GameResolver => resolver;

    public async ValueTask<IGameProcessWatcher> LaunchAsync(string id) {
        var gameEntry = GameResolver.GetGameEntity(id);
        var versionPath = gameEntry.OfVersionDirectoryPath(config.IsEnableIndependencyCore);
        _argumentsBuilder = new(gameEntry, config);

        var arguments = _argumentsBuilder.Build();
        var process = CreateProcess(arguments, versionPath);

        LibrariesResolver librariesResolver = new(gameEntry);
        await ExtractNativesAndStartProcess(versionPath, librariesResolver, process);
        return new GameProcessWatcher(process, arguments);
    }

    private Process CreateProcess(IEnumerable<string> arguments, string versionPath) {
        return new Process {
            StartInfo = new ProcessStartInfo {
                FileName = LaunchConfig.JvmConfig.JavaPath.FullName,
                Arguments = string.Join(' '.ToString(), arguments),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = versionPath
            },

            EnableRaisingEvents = true
        };
    }

    private async Task ExtractNativesAndStartProcess(string versionPath, LibrariesResolver librariesResolver, Process process) {
        var libraries = librariesResolver.GetLibraries()
            .Where(x => ((x as LibraryEntry)?.IsNative) != null)
            .Select(x => x.Path)
            .ToList();

        await Task.Run(() => ZipUtil.ExtractNatives(Path.Combine(versionPath, "natives"), libraries));
        process.Start();
    }
}