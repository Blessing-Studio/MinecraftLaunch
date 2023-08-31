using MinecraftLaunch.Modules.Enum;
using System.Text.Json;
using System.Text.Json.Serialization;

//using System.Text.Json.Serialization;
namespace MinecraftLaunch.Modules.Models.Auth;

[JsonConverter(typeof(AccountJsonConverter))]
public abstract class Account {
    public string Name { get; set; }

    public Guid Uuid { get; set; }

    public string AccessToken { get; set; }

    public string ClientToken { get; set; }

    public virtual AccountType Type { get; set; }

    public static OfflineAccount Default { get; private set; } = new OfflineAccount {
        Name = "Steve",
        Uuid = Guid.NewGuid(),
        AccessToken = Guid.NewGuid().ToString("N"),
        ClientToken = Guid.NewGuid().ToString("N")
    };


    public Account() {
    }

    public Account(string Name, string uuid, string accesstoken, string clienttoken = null) {
        if (string.IsNullOrEmpty(clienttoken)) {
            clienttoken = Guid.NewGuid().ToString("N");
        }
        if (string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(uuid) || string.IsNullOrEmpty(accesstoken)) {
            throw new ArgumentNullException("", "name或uuid或token为null！");
        }
        Guid token = Guid.NewGuid();
        if (!Guid.TryParse(accesstoken, out token) || !Guid.TryParse(clienttoken, out token) || !Guid.TryParse(uuid, out token)) {
            throw new ArgumentException("uuid或accesstoken或clienttoken的格式错误！");
        }
    }

    public static implicit operator Account(string name) => new OfflineAccount(name);
}

public class AccountJsonConverter : JsonConverter<Account> {
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
                ClientToken = root.GetProperty("ClientToken").GetString(),
                Name = root.GetProperty("Name").GetString(),
                Uuid = root.GetProperty("Uuid").GetGuid()
            },
            AccountType.Microsoft => new MicrosoftAccount {
                AccessToken = root.GetProperty("AccessToken").GetString()!,
                ClientToken = root.GetProperty("ClientToken").GetString(),
                Name = root.GetProperty("Name").GetString(),
                Uuid = root.GetProperty("Uuid").GetGuid(),
                DateTime = root.GetProperty("DateTime").GetDateTime(),
                RefreshToken = root.GetProperty("RefreshToken").GetString()
            },
            AccountType.Yggdrasil => new YggdrasilAccount {
                AccessToken = root.GetProperty("AccessToken").GetString(),
                ClientToken = root.GetProperty("ClientToken").GetString(),
                Name = root.GetProperty("Name").GetString(),
                Uuid = root.GetProperty("Uuid").GetGuid(),
                YggdrasilServerUrl = root.GetProperty("YggdrasilServerUrl").GetString()
            },
            _ => default!
        };
    }

    public override void Write(Utf8JsonWriter writer, Account value, JsonSerializerOptions options) {
        throw new NotImplementedException();
    }
}
