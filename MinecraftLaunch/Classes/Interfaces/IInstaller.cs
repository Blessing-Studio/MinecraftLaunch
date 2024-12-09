using MinecraftLaunch.Classes.Models.Event;

namespace MinecraftLaunch.Classes.Interfaces;

public interface IInstaller {

    /// <summary>
    /// 异步安装方法
    /// </summary>
    /// <returns></returns>
    Task<bool> InstallAsync(CancellationToken cancellation = default);

    /// <summary>
    /// 安装完成事件
    /// </summary>
    event EventHandler<EventArgs> Completed;

    /// <summary>
    /// 安装进度改变事件
    /// </summary>
    event EventHandler<ProgressChangedEventArgs> ProgressChanged;
}