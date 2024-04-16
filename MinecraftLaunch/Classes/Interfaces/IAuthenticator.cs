using MinecraftLaunch.Classes.Models.Auth;

namespace MinecraftLaunch.Classes.Interfaces;

/// <summary>
/// 验证器统一接口
/// </summary>
public interface IAuthenticator<out TAccount> where TAccount : Account {
    /// <summary>
    /// 验证方法
    /// </summary>
    /// <returns></returns>
    TAccount Authenticate();
}