using MinecraftLaunch.Classes.Models.Launch;

namespace MinecraftLaunch.Classes.Interfaces;

/// <summary>
/// 启动器统一接口（IoC适应）
/// </summary>
public interface ILauncher {
    LaunchConfig LaunchConfig { get; }

    IGameResolver GameResolver { get; }

    ValueTask<IGameProcessWatcher> LaunchAsync(string id);
}