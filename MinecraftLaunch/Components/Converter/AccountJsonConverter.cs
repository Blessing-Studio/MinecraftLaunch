using System.Text.Json;
using System.Text.Json.Serialization;
using MinecraftLaunch.Classes.Enums;
using MinecraftLaunch.Classes.Models.Auth;

namespace MinecraftLaunch.Components.Converter;

public sealed class AccountJsonConverter : JsonConverter<Account> {
    public override bool CanConvert(Type typeToConvert) {
        return typeToConvert == typeof(Account);
    }

    public override Account Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        using var jsonDoc = JsonDocument.ParseValue(ref reader);
        var root = jsonDoc.RootElement;
        var accountType = (AccountType)root.GetProperty("Type").GetInt32();

        return accountType switch {
            AccountType.Offline => new OfflineAccount {
                AccessToken = root.GetProperty("AccessToken").GetString(),
                Name = root.GetProperty("Name").GetString(),
                Uuid = root.GetProperty("Uuid").GetGuid()
            },
            AccountType.Microsoft => new MicrosoftAccount {
                AccessToken = root.GetProperty("AccessToken").GetString()!,
                Name = root.GetProperty("Name").GetString(),
                Uuid = root.GetProperty("Uuid").GetGuid(),
                RefreshToken = root.GetProperty("RefreshToken").GetString()
            },
            AccountType.Yggdrasil => new YggdrasilAccount {
                AccessToken = root.GetProperty("AccessToken").GetString(),
                ClientToken = root.GetProperty("ClientToken").GetString(),
                Name = root.GetProperty("Name").GetString(),
                Uuid = root.GetProperty("Uuid").GetGuid(),
                YggdrasilServerUrl = root.GetProperty("YggdrasilServerUrl").GetString()
            },
            AccountType.UnifiedPass => new UnifiedPassAccount {
                AccessToken = root.GetProperty("AccessToken").GetString(),
                ClientToken = root.GetProperty("ClientToken").GetString(),
                Name = root.GetProperty("Name").GetString(),
                Uuid = root.GetProperty("Uuid").GetGuid(),
                ServerId = root.GetProperty("ServerId").GetString()
            },
            _ => default!
        };
    }

    public override void Write(Utf8JsonWriter writer, Account value, JsonSerializerOptions options) {
        throw new NotImplementedException();
    }
}