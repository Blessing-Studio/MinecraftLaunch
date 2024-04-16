using System.Diagnostics;
using MinecraftLaunch.Classes.Models.Event;

namespace MinecraftLaunch.Classes.Interfaces;

public interface IGameProcessWatcher {
    /// <summary>
    /// 游戏进程
    /// </summary>
    Process Process { get; }

    /// <summary>
    /// 完整的游戏启动参数
    /// </summary>
    IEnumerable<string> Arguments { get; }

    /// <summary>
    /// 游戏退出事件
    /// </summary>
    event EventHandler<ExitedEventArgs> Exited;

    /// <summary>
    /// 游戏日志输出事件
    /// </summary>
    event EventHandler<LogReceivedEventArgs> OutputLogReceived;
}