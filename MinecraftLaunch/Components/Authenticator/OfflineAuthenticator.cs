using System.Text;
using System.Security.Cryptography;
using MinecraftLaunch.Classes.Interfaces;
using MinecraftLaunch.Classes.Models.Auth;

namespace MinecraftLaunch.Components.Authenticator;

/// <summary>
/// 离线账户身份验证器
/// </summary>
/// <param name="name"></param>
/// <param name="uuid"></param>
public sealed class OfflineAuthenticator(string name, Guid? uuid = default) : IAuthenticator<OfflineAccount> {
    public OfflineAccount Authenticate() {
        return new() {
            AccessToken = Guid.NewGuid().ToString("N"),
            Name = name,
            Uuid = uuid ?? new(MD5.HashData(Encoding.UTF8.GetBytes(name)))
        };
    }
}