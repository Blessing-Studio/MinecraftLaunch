using System.Text.Json.Serialization;

namespace MinecraftLaunch.Classes.Models.Auth;

public record DeviceCodeResponse {
    [JsonPropertyName("user_code")]
    public string UserCode { get; set; }

    [JsonPropertyName("device_code")]
    public string DeviceCode { get; set; }

    [JsonPropertyName("verification_uri")]
    public string VerificationUrl { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("interval")]
    public int Interval { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }
}

[JsonSerializable(typeof(DeviceCodeResponse))]
partial class DeviceCodeResponseContext : JsonSerializerContext;