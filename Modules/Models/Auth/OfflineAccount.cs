using MinecraftLaunch.Modules.Enum;

namespace MinecraftLaunch.Modules.Models.Auth;

public class OfflineAccount : Account
{
	public override AccountType Type => AccountType.Offline;

	public OfflineAccount()
	{
	}

	public OfflineAccount(string Name, string uuid, string accesstoken, string clienttoken = null)
		: base(Name, uuid, accesstoken, clienttoken)
	{
		Console.WriteLine(this.Name);
		Console.WriteLine(this.Uuid);
		Console.WriteLine(this.AccessToken);
    }
}
