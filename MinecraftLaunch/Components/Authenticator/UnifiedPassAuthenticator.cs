using Flurl.Http;
using MinecraftLaunch.Classes.Interfaces;
using MinecraftLaunch.Classes.Models.Auth;
using MinecraftLaunch.Extensions;

namespace MinecraftLaunch.Components.Authenticator;

public sealed class UnifiedPassAuthenticator(string serverId, string userName, string passWord) : IAuthenticator<UnifiedPassAccount> {
    private string _serverId = serverId;
    private string _userName = userName;
    private string _passWord = passWord;
    private readonly string _baseUrl = "https://auth.mc-user.com:233/";

    public void RefreshInformation(string serverId, string userName, string passWord) {
        _serverId = serverId;
        _passWord = passWord;
        _userName = userName;
    }
    
    public UnifiedPassAccount Authenticate() {
        return AuthenticateAsync()
            .GetAwaiter()
            .GetResult();
    }

    public async ValueTask<UnifiedPassAccount> AuthenticateAsync() {
        string authUrl = $"{_baseUrl}{_serverId}/authserver/authenticate";
        var content = new {
            agent = new {
                name = "MinecraftLaunch",
                version = 1.00
            },
            username = _userName,
            password = _passWord,
            clientToken = null as string,
            requestUser = true,
        };
        
        var node = (await (await authUrl.PostJsonAsync(content))
            .GetStringAsync())
            .AsNode();

        var user = node.GetEnumerable("availableProfiles")
            .FirstOrDefault();

        return new UnifiedPassAccount {
            ServerId = _serverId,
            Name = user.GetString("name"),
            Uuid = Guid.Parse(user.GetString("id")),
            AccessToken = node.GetString("accessToken"),
            ClientToken = node.GetString("clientToken"),
        };
    }
}