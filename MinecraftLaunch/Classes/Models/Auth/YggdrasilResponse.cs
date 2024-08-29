using System.Text.Json.Serialization;

namespace MinecraftLaunch.Classes.Models.Auth;

public sealed record YggdrasilResponse {
    [JsonPropertyName("user")]
    public User User { get; set; }

    [JsonPropertyName("clientToken")]
    public string ClientToken { get; set; }

    [JsonPropertyName("accessToken")]
    public string AccessToken { get; set; }

    [JsonPropertyName("availableProfiles")]
    public IEnumerable<ProfileModel> AvailableProfiles { get; set; }

    [JsonPropertyName("selectedProfile")]
    public ProfileModel SelectedProfile { get; set; }
}

public sealed record User {
    [JsonPropertyName("properties")]
    public IEnumerable<PropertyModel> Properties { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }
}

public sealed record PropertyModel {
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("profileId")]
    public string ProfileId { get; set; }

    [JsonPropertyName("userId")]
    public string UserId { get; set; }

    [JsonPropertyName("value")]
    public string Value { get; set; }
}

public sealed record ProfileModel {
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }
}

[JsonSerializable(typeof(YggdrasilResponse))]
internal sealed partial class YggdrasilResponseContext : JsonSerializerContext;