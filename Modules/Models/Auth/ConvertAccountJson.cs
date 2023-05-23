using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MinecraftLaunch.Modules.Models.Auth;

public class ConvertAccountJson : JsonConverter
{
	public override bool CanRead => true;

	public override bool CanWrite => false;

	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(Account);
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		JObject jObject = serializer.Deserialize<JObject>(reader);
		if (jObject == null)
		{
			return null;
		}
		return Extensions.Value<int>((IEnumerable<JToken>)jObject["Type"]) switch
		{
			1 => new OfflineAccount
			{
				AccessToken = jObject["AccessToken"].ToObject<string>(),
				ClientToken = jObject["ClientToken"].ToObject<string>(),
				Name = jObject["Name"].ToObject<string>(),
				Uuid = jObject["Uuid"].ToObject<Guid>()
			}, 
			2 => new MicrosoftAccount
			{
				AccessToken = jObject["AccessToken"].ToObject<string>(),
				ClientToken = jObject["ClientToken"].ToObject<string>(),
				Name = jObject["Name"].ToObject<string>(),
				Uuid = jObject["Uuid"].ToObject<Guid>(),
				DateTime = jObject["DateTime"].ToObject<DateTime>(),
				RefreshToken = jObject["RefreshToken"].ToObject<string>()
			}, 
			3 => new YggdrasilAccount
			{
				AccessToken = jObject["AccessToken"].ToObject<string>(),
				ClientToken = jObject["ClientToken"].ToObject<string>(),
				Name = jObject["Name"].ToObject<string>(),
				Uuid = jObject["Uuid"].ToObject<Guid>(),
				YggdrasilServerUrl = jObject["YggdrasilServerUrl"].ToObject<string>()
			}, 
			_ => null, 
		};
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		throw new NotImplementedException();
	}
}
