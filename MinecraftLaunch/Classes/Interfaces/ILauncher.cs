using MinecraftLaunch.Classes.Models.Launch;

namespace MinecraftLaunch.Classes.Interfaces;

/// <summary>
/// 启动器统一接口
/// </summary>
public interface ILauncher {

    /// <summary>
    /// 游戏启动配置
    /// </summary>
    LaunchConfig LaunchConfig { get; }

    /// <summary>
    /// 游戏实例解析器
    /// </summary>
    IGameResolver GameResolver { get; }

    /// <summary>
    /// 异步游戏启动方法
    /// </summary>
    ValueTask<IGameProcessWatcher> LaunchAsync(string id);
}