using System;
using MinecraftLaunch.Modules.Enum;
using Newtonsoft.Json;

namespace MinecraftLaunch.Modules.Models.Auth;

[JsonConverter(typeof(ConvertAccountJson))]
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
}
