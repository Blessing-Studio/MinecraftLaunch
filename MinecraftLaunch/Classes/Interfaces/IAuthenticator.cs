namespace MinecraftLaunch.Classes.Interfaces {
    /// <summary>
    /// 验证器统一接口（IoC 适应）
    /// </summary>
    public interface IAuthenticator<TAccount> {
        TAccount Authenticate();

        ValueTask<TAccount> AuthenticateAsync();
    }
}
