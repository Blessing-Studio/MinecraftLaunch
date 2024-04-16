namespace MinecraftLaunch.Classes.Interfaces;

/// <summary>
/// 检查器统一接口
/// </summary>
public interface IChecker {
    /// <summary>
    /// 异步检查方法
    /// </summary>
    ValueTask<bool> CheckAsync();
}