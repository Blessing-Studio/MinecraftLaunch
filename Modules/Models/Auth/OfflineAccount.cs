using MinecraftLaunch.Modules.Enum;

namespace MinecraftLaunch.Modules.Models.Auth;

public class OfflineAccount : Account
{
	public override AccountType Type => AccountType.Offline;

	public OfflineAccount(){}

	public OfflineAccount(string name)
	{
		AccessToken = Guid.NewGuid().ToString("N");
		ClientToken = Guid.NewGuid().ToString("N");
		Name = name;
		Uuid = Guid.NewGuid();
    }

	public OfflineAccount(string Name, string uuid, string accesstoken, string clienttoken = null)
		: base(Name, uuid, accesstoken, clienttoken)
	{
    }
}
