using System;
using MinecraftLaunch.Modules.Enum;

namespace MinecraftLaunch.Modules.Models.Auth;

public class MicrosoftAccount : Account
{
	public override AccountType Type => AccountType.Microsoft;

	public string? RefreshToken { get; set; }

	public DateTime DateTime { get; set; }

	public MicrosoftAccount()
	{
	}

	public MicrosoftAccount(string Name, string uuid, string accesstoken, string clienttoken = null)
		: base(Name, uuid, accesstoken, clienttoken)
	{
	}
}
