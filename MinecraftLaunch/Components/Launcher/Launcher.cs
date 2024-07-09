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
/// Launcher for the Java version of Minecraft.
/// </summary>
public sealed class Launcher : ILauncher {
    private ArgumentsBuilder _argumentsBuilder;

    /// <summary>
    /// Gets the launch configuration.
    /// </summary>
    public LaunchConfig LaunchConfig { get; }

    public IGameResolver GameResolver { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Launcher"/> class.
    /// </summary>
    /// <param name="resolver">The game resolver.</param>
    /// <param name="config">The launch configuration.</param>
    public Launcher(IGameResolver resolver, LaunchConfig config) {
        GameResolver = resolver;
        LaunchConfig = config;
    }

    /// <summary>
    /// Launches the game asynchronously.
    /// </summary>
    /// <param name="id">The ID of the game to launch.</param>
    /// <returns>A ValueTask that represents the asynchronous operation. The task result contains a <see cref="GameProcessWatcher"/>.</returns>
    public async ValueTask<IGameProcessWatcher> LaunchAsync(string id) {
    var gameEntry = GameResolver.GetGameEntity(id);
        var versionPath = gameEntry.OfVersionDirectoryPath(LaunchConfig.IsEnableIndependencyCore);
        _argumentsBuilder = new(gameEntry, LaunchConfig);
        
        var arguments = _argumentsBuilder.Build();
        var process = CreateProcess(arguments, versionPath);

        LibrariesResolver librariesResolver = new(gameEntry);
        await Launcher.ExtractNatives(versionPath, librariesResolver);
        return new GameProcessWatcher(process, arguments);
    }

    private Process CreateProcess(IEnumerable<string> arguments, string versionPath) {
        return new Process {
            EnableRaisingEvents = true,
            StartInfo = new ProcessStartInfo {
                FileName = LaunchConfig.JvmConfig.JavaPath.FullName,
                Arguments = string.Join(' '.ToString(), arguments),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = versionPath
            },
        };
    }

    private static async Task ExtractNatives(string versionPath, LibrariesResolver librariesResolver) {
        var libraries = librariesResolver.GetLibraries()
            .Where(x => ((x as LibraryEntry)?.IsNative) != null)
            .Select(x => x.Path)
            .ToList();

        await Task.Run(() => ZipUtil.ExtractNatives(Path.Combine(versionPath, "natives"), libraries));
    }
}