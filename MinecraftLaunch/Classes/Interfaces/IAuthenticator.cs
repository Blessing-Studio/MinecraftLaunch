namespace MinecraftLaunch.Classes.Interfaces;

/// <summary>
/// 验证器统一接口
/// </summary>
public interface IAuthenticator<out TAccount> {

    /// <summary>
    /// 验证方法
    /// </summary>
    /// <returns></returns>
    TAccount Authenticate();
}