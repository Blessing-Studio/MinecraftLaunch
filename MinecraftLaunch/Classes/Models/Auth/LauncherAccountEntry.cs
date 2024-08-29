using System.Text.Json.Serialization;

namespace MinecraftLaunch.Classes.Models.Auth;

public sealed record LauncherAccountEntry {
    [JsonPropertyName("mojangClientToken")]
    public string MojangClientToken { get; set; }

    [JsonPropertyName("activeAccountLocalId")]
    public string ActiveAccountLocalId { get; set; }

    [JsonPropertyName("accounts")]
    public Dictionary<string, AccountEntry> Accounts { get; set; }
}

public sealed record AccountEntry {
    [JsonPropertyName("accessToken")]
    public string AccessToken { get; set; }

    [JsonPropertyName("accessTokenExpiresAt")]
    public DateTime AccessTokenExpiresAt { get; set; }

    [JsonPropertyName("avatar")]
    public string AvatarUrl { get; set; }

    [JsonPropertyName("eligibleForMigration")]
    public bool IsEligibleForMigration { get; set; }

    [JsonPropertyName("hasMultipleProfiles")]
    public bool HasMultipleProfiles { get; set; }

    [JsonPropertyName("legacy")]
    public bool IsLegacy { get; set; }

    [JsonPropertyName("localId")]
    public string LocalId { get; set; }

    [JsonPropertyName("minecraftProfile")]
    public AccountProfileEntry MinecraftProfile { get; set; }

    [JsonPropertyName("persistent")]
    public bool IsPersistent { get; set; }

    [JsonPropertyName("remoteId")]
    public string RemoteId { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("userProperites")]
    public AccountProfileEntry[] UserProperites { get; set; }

    [JsonPropertyName("username")]
    public string UserName { get; set; }

    [JsonPropertyName("__id")]
    public Guid Id { get; set; }
}

public sealed record AccountProfileEntry {
    [JsonPropertyName("id")]
    public string Uuid { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }
}

[JsonSerializable(typeof(LauncherAccountEntry))]
internal partial class LauncherAccountEntryContext : JsonSerializerContext;