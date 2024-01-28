namespace MinecraftLaunch.Classes.Interfaces;

/// <summary>
/// 验证器统一接口（IoC 适应）
/// </summary>
public interface IAuthenticator<out TAccount> {
    TAccount Authenticate();
}