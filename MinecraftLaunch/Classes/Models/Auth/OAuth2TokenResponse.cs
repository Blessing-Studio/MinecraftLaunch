using System.Text.Json.Serialization;

namespace MinecraftLaunch.Classes.Models.Auth;

public sealed record OAuth2TokenResponse {
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("scope")]
    public string Scope { get; set; }

    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; }

    [JsonPropertyName("user_id")]
    public string UserId { get; set; }

    [JsonPropertyName("foci")]
    public string Foci { get; set; }
}
    
[JsonSerializable(typeof(OAuth2TokenResponse))]
sealed partial class OAuth2TokenResponseContext : JsonSerializerContext;