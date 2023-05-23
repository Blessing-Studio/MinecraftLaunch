using System;
using MinecraftLaunch.Modules.Enum;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using JsonConverter = Newtonsoft.Json.JsonConverter;
using JsonConverterAttribute = Newtonsoft.Json.JsonConverterAttribute;

namespace MinecraftLaunch.Modules.Models.Auth;

[JsonConverter(typeof(AccountJsonConverter))]
public abstract class Account
{
	public string Name { get; set; }

	public Guid Uuid { get; set; }

	public string AccessToken { get; set; }

	public string ClientToken { get; set; }

	public virtual AccountType Type { get; set; }

	public static OfflineAccount Default { get; private set; } = new OfflineAccount
	{
		Name = "Steve",
		Uuid = Guid.NewGuid(),
		AccessToken = Guid.NewGuid().ToString("N"),
		ClientToken = Guid.NewGuid().ToString("N")
	};


	public Account()
	{
	}

	public Account(string Name, string uuid, string accesstoken, string clienttoken = null)
	{
		if (string.IsNullOrEmpty(clienttoken))
		{
			clienttoken = Guid.NewGuid().ToString("N");
		}
		if (string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(uuid) || string.IsNullOrEmpty(accesstoken))
		{
			throw new ArgumentNullException("", "name或uuid或token为null！");
		}
		Guid token = Guid.NewGuid();
		if (!Guid.TryParse(accesstoken, out token) || !Guid.TryParse(clienttoken, out token) || !Guid.TryParse(uuid, out token))
		{
			throw new ArgumentException("uuid或accesstoken或clienttoken的格式错误！");
		}
	}

    public static implicit operator Account(string name) => new OfflineAccount(name);
}

public class AccountJsonConverter : JsonConverter
{
    public override bool CanConvert(Type objectType) => objectType == typeof(Account);

    public override bool CanRead => true;

    public override bool CanWrite => false;

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var jobject = serializer.Deserialize<JObject>(reader);

        if (jobject == null)
            return null;

        var accountType = (AccountType)jobject["Type"].Value<int>();

        return accountType switch
        {
            AccountType.Offline => new OfflineAccount
            {
                AccessToken = jobject["AccessToken"].ToObject<string>(),
                ClientToken = jobject["ClientToken"].ToObject<string>(),
                Name = jobject["Name"].ToObject<string>(),
                Uuid = jobject["Uuid"].ToObject<Guid>()
            },
            AccountType.Microsoft => new MicrosoftAccount
            {
                AccessToken = jobject["AccessToken"].ToObject<string>(),
                ClientToken = jobject["ClientToken"].ToObject<string>(),
                Name = jobject["Name"].ToObject<string>(),
                Uuid = jobject["Uuid"].ToObject<Guid>(),
                DateTime = jobject["DateTime"].ToObject<DateTime>(),
                RefreshToken = jobject["RefreshToken"].ToObject<string>()
            },
            AccountType.Yggdrasil => new YggdrasilAccount
            {
                AccessToken = jobject["AccessToken"].ToObject<string>(),
                ClientToken = jobject["ClientToken"].ToObject<string>(),
                Name = jobject["Name"].ToObject<string>(),
                Uuid = jobject["Uuid"].ToObject<Guid>(),
                YggdrasilServerUrl = jobject["YggdrasilServerUrl"].ToObject<string>()
            },
            _ => null
        };
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();
}

