using Newtonsoft.Json;

namespace MinecraftLaunch.Modules.Models.Auth;

public class TokenResponse
{
    /// <summary>
    /// 令牌类型
    /// </summary>
    [JsonProperty("token_type")]public string TokenType { get; set; }
    /// <summary>
    /// 范围
    /// </summary>
    [JsonProperty("scope")]public string Scope { get; set; }
    /// <summary>
    /// 到期时间
    /// </summary>
    [JsonProperty("expires_in")]public string ExpiresIn { get; set; }
    /// <summary>
    ///  我不到啊
    /// </summary>
    [JsonProperty("ext_expires_in")]public string ExtexpiresIn { get; set; }
    /// <summary>
    /// 我不到啊
    /// </summary>
    [JsonProperty("expires_on")]public string ExpiresOn { get; set; }
    /// <summary>
    /// 我不到啊
    /// </summary>
    [JsonProperty("not_before")]public string NotBefore { get; set; }
    /// <summary>
    /// 资源
    /// </summary>
    [JsonProperty("resource")]public string Resource { get; set; }
    /// <summary>
    /// 访问令牌
    /// </summary>
    [JsonProperty("access_token")]public string AccessToken { get; set; }
    /// <summary>
    /// 刷新令牌
    /// </summary>
    [JsonProperty("refresh_token")]public string RefreshToken { get; set; }
    /// <summary>
    /// Id令牌
    /// </summary>
    [JsonProperty("id_token")]public string IdToken { get; set; }
    /// <summary>
    /// 错误
    /// </summary>
    [JsonProperty("error")]public string Error { get; set; }
}