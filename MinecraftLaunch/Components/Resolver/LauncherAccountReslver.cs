using System.Text;
using MinecraftLaunch.Classes.Interfaces;
using MinecraftLaunch.Classes.Models.Auth;
using MinecraftLaunch.Classes.Models.Game;
using MinecraftLaunch.Extensions;
using MinecraftLaunch.Utilities;

namespace MinecraftLaunch.Components.Resolver;

/// <summary>
/// 官方游戏账户解析器
/// </summary>
/// <remarks>
/// 取自 launcher_accounts.json
/// </remarks>
public sealed class LauncherAccountReslver(string rootPath, Guid clientToken = default) {
    private readonly Guid _clientToken = clientToken;
    private readonly string _accountPath = rootPath.OfLauncherAccountPath();
    
    public LauncherAccountEntry LauncherAccount { get; set; }
    
    public LauncherAccountEntry Resolve(string str = default) {
        if (File.Exists(_accountPath)) {
            var launcherAccountJson = File.ReadAllText(_accountPath, Encoding.UTF8);
            LauncherAccount = launcherAccountJson.Deserialize(LauncherAccountEntryContext
                .Default.LauncherAccountEntry);
            
            return LauncherAccount;
        }
        
        var launcherAccount = new LauncherAccountEntry {
            Accounts = new(),
            MojangClientToken = _clientToken.ToString("N")
        };
        
        LauncherAccount = launcherAccount;
        string profileJson = LauncherAccount.Serialize(typeof(LauncherProfileEntry), 
            new LauncherProfileEntryContext(JsonConverterUtil.DefaultJsonOptions));

        if (!Directory.Exists(rootPath)) {
            Directory.CreateDirectory(rootPath);
        }

        File.WriteAllText(_accountPath, profileJson);
        return LauncherAccount;
    }
    
    public void Save() {
        var launcherProfileJson = LauncherAccount.Serialize(typeof(LauncherAccountEntry), 
            new LauncherAccountEntryContext(JsonConverterUtil.DefaultJsonOptions));
        
        File.WriteAllText(_accountPath, launcherProfileJson);
    }
    
    public bool Remove(Guid id)
    {
        var result = Find(id);
        if (!result.HasValue) {
            return false;
        }

        var (key, value) = result.Value;
        if (value == default) {
            return false;
        }
        
        return LauncherAccount.Accounts.Remove(key);
    }

    public bool Select(string uuid) {
        if (!(LauncherAccount?.Accounts?.ContainsKey(uuid) ?? false)) {
            return false;
        }

        LauncherAccount.ActiveAccountLocalId = uuid;
        return true;
    }
    
    public Guid Add(string uuid, AccountEntry account) {
        if (LauncherAccount == null || LauncherAccount.Accounts?.ContainsKey(uuid) == true) {
            return default;
        }

        var oldRecord = LauncherAccount.Accounts
            !.FirstOrDefault(a => 
                a.Value.MinecraftProfile.Uuid == account.MinecraftProfile.Uuid).Value;

        if (oldRecord != null) {
            return oldRecord.Id;
        }

        var findResult = Find(account.Id);
        if (findResult is { Key: not null, Value: not null }) {
            LauncherAccount.Accounts[findResult.Value.Key] = account;
            return account.Id;
        }
        else {
            Guid newId = account.Id == default ? Guid.NewGuid() : account.Id;
            LauncherAccount.Accounts.Add(uuid, account);
            return newId;
        }
    }
    
    public KeyValuePair<string, AccountEntry>? Find(Guid id) {
        return LauncherAccount?.Accounts?.FirstOrDefault(a => a.Value.Id == id);
    }
}