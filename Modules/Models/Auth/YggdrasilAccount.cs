using MinecraftLaunch.Modules.Enum;

namespace MinecraftLaunch.Modules.Models.Auth;

public class YggdrasilAccount : Account
{
	public override AccountType Type => AccountType.Yggdrasil;

	public string YggdrasilServerUrl { get; set; }

	public string Email { get; set; }

	public string Password { get; set; }

	public YggdrasilAccount()
	{
	}

	public YggdrasilAccount(string Name, string uuid, string accesstoken, string clienttoken = null)
		: base(Name, uuid, accesstoken, clienttoken)
	{
	}
}
