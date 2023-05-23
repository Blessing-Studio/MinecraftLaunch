using MinecraftLaunch.Modules.Models;
using MinecraftLaunch.Modules.Models.Auth;
using Natsurainko.Toolkits.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftLaunch.Modules.Authenticator
{
    /// <summary>
    /// 离线验证器
    /// </summary>
    public partial class OfflineAuthenticator : AuthenticatorBase
    {
        public override OfflineAccount Auth() => new OfflineAccount
        {
            AccessToken = Guid.NewGuid().ToString("N"),
            ClientToken = Guid.NewGuid().ToString("N"),
            Name = this.Name,
            Uuid = this.Uuid
        };

        public async ValueTask<OfflineAccount> AuthAsync(Action<string> func = default) => await Task.FromResult(new OfflineAccount
        {
            AccessToken = Guid.NewGuid().ToString("N"),
            ClientToken = Guid.NewGuid().ToString("N"),
            Name = this.Name,
            Uuid = this.Uuid
        });
    }

    partial class OfflineAuthenticator
    {
        public OfflineAuthenticator(string name, Guid uuid = default)
        {
            this.Name = name;
            this.Uuid = uuid;

            if (this.Uuid == default)
                this.Uuid = GuidHelper.FromString(this.Name);
        }

        public string Name { get; set; }

        public Guid Uuid { get; set; }
    }
}
