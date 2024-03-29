﻿using Flurl.Http;
using MinecraftLaunch.Extensions;
using MinecraftLaunch.Classes.Interfaces;
using MinecraftLaunch.Classes.Models.Auth;

namespace MinecraftLaunch.Components.Authenticator;

public sealed class YggdrasilAuthenticator(YggdrasilAccount account) : IAuthenticator<IEnumerable<YggdrasilAccount>> {
    private readonly string _email;
    private readonly string _password;
    private readonly YggdrasilAccount _account = account;
    private readonly string _url = account?.YggdrasilServerUrl;

    public YggdrasilAuthenticator(string url, string email, string password) : this(default) {
        _url = url;
        _email = email;
        _password = password;
    }

    public IEnumerable<YggdrasilAccount> Authenticate() {
        return AuthenticateAsync().GetAwaiter().GetResult();
    }

    public async ValueTask<IEnumerable<YggdrasilAccount>> AuthenticateAsync() {
        object payload = string.Empty;
        string url = _url;

        if (_account is null) {
            url += "/authserver/authenticate";
            payload = new {
                clientToken = Guid.NewGuid().ToString("N"),
                username = _email,
                password = _password,
                requestUser = false,
                agent = new {
                    name = "Minecraft",
                    version = 1
                }
            };
        } else {
            url += "/authserver/refresh";
            payload = new {
                ClientToken = _account.ClientToken,
                accessToken = _account.AccessToken,
                requestUser = true
            };
        }

        var json = await url.PostJsonAsync(payload)
            .ReceiveString();

        var entry = json.Deserialize(YggdrasilResponseContext.Default.YggdrasilResponse);
        return entry.AvailableProfiles.Select(profile => new YggdrasilAccount {
            Name = profile.Name,
            YggdrasilServerUrl = _url,
            Uuid = Guid.Parse(profile.Id),
            ClientToken = entry.ClientToken,
            AccessToken = entry.AccessToken,
        }).ToArray();
    }
}