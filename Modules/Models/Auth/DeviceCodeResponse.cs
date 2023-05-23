using Newtonsoft.Json;

namespace MinecraftLaunch.Modules.Models.Auth;
/// <summary>
/// 设备代码响应模型类
/// </summary>
public class DeviceCodeResponse
{
    /// <summary>
    /// 用户代码
    /// </summary>
    [JsonProperty("user_code")]
    public string UserCode { get; set; }
    /// <summary>
    /// 设备代码
    /// </summary>
    [JsonProperty("device_code")]
    public string DeviceCode { get; set; }
    /// <summary>
    /// 验证网址
    /// </summary>
    [JsonProperty("verification_uri")]
    public string VerificationUrl { get; set; }
    /// <summary>
    /// 过期时间
    /// </summary>
    [JsonProperty("expires_in")]
    public int ExpiresIn { get; set; }
    /// <summary>
    /// 间隔
    /// </summary>
    [JsonProperty("interval")]
    public int Interval { get; set; }
    /// <summary>
    /// 消息
    /// </summary>
    [JsonProperty("message")]
    public string Message { get; set; }
}