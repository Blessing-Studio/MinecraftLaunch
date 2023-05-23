using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftLaunch.Modules.Models.Auth
{
    public class YggdrasilResponse
    {
        /// <summary>
        /// 验证令牌
        /// </summary>
        [JsonProperty("accessToken")] public string AccessToken { get; internal set; }

        [JsonProperty("clientToken")] public string ClientToken { get; internal set; }
        /// <summary>
        /// 用户在皮肤站注册的所有游戏账号
        /// </summary>
        [JsonProperty("availableProfiles")] public List<AvailableProfiles> UserAccounts { get; internal set; }
    }
}

public class AvailableProfiles
{
    /// <summary>
    /// 游戏角色Uuid
    /// </summary>
    [JsonProperty("id")]public string Uuid { get; set; }
    /// <summary>
    /// 游戏角色名
    /// </summary>
    [JsonProperty("name")] public string Name { get; set; }
}
