using Flurl.Http;
using MinecraftLaunch.Classes.Interfaces;
using MinecraftLaunch.Classes.Models.Auth;
using MinecraftLaunch.Extensions;

namespace MinecraftLaunch.Components.Authenticator;

/// <summary>
/// Authenticator for UnifiedPass accounts.
/// </summary>
/// <param name="serverId">The server ID for the account.</param>
/// <param name="userName">The username of the account.</param>
/// <param name="passWord">The password of the account.</param>
public sealed class UnifiedPassAuthenticator(string serverId, string userName, string passWord) : IAuthenticator<UnifiedPassAccount> {
    private const string BASE_URL = "https://auth.mc-user.com:233/";

    private string _serverId = serverId;
    private string _userName = userName;
    private string _passWord = passWord;

    /// <summary>
    /// Refreshes the information of the account.
    /// </summary>
    /// <param name="serverId">The new server ID for the account.</param>
    /// <param name="userName">The new username of the account.</param>
    /// <param name="passWord">The new password of the account.</param>
    public void RefreshInformation(string serverId, string userName, string passWord) {
        _serverId = serverId;
        _passWord = passWord;
        _userName = userName;
    }

    /// <summary>
    /// Authenticates the UnifiedPass account.
    /// </summary>
    /// <returns>The authenticated UnifiedPass account.</returns>
    public UnifiedPassAccount Authenticate() {
        return AuthenticateAsync()
            .GetAwaiter()
            .GetResult();
    }

    /// <summary>
    /// Asynchronously authenticates the UnifiedPass account.
    /// </summary>
    /// <returns>A ValueTask that represents the asynchronous operation. The task result contains the authenticated UnifiedPass account.</returns>
    public async ValueTask<UnifiedPassAccount> AuthenticateAsync() {
        string authUrl = $"{BASE_URL}{_serverId}/authserver/authenticate";
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