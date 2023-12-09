using MinecraftLaunch.Classes.Interfaces;
using MinecraftLaunch.Classes.Models.Auth;
using System.Xml.Linq;
using System;

namespace MinecraftLaunch.Components.Authenticator {
    /// <summary>
    /// 离线账户身份验证器
    /// </summary>
    /// <param name="name"></param>
    /// <param name="uuid"></param>
    public class OfflineAuthenticator(string name, Guid uuid = default) : IAuthenticator<OfflineAccount> {
        public OfflineAccount Authenticate() => new() {
            AccessToken = Guid.NewGuid().ToString("N"),
            Name = name,
            Uuid = uuid
        };

        async ValueTask<OfflineAccount> IAuthenticator<OfflineAccount>.AuthenticateAsync() {
            return await Task.FromResult(Authenticate());
        }
    }
}
