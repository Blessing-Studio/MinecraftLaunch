using Flurl.Http;
using MinecraftLaunch.Extensions;
using MinecraftLaunch.Classes.Enums;
using MinecraftLaunch.Classes.Interfaces;
using MinecraftLaunch.Classes.Models.Auth;

namespace MinecraftLaunch.Components.Authenticator;

public sealed class MicrosoftAuthenticator(string clientId, bool isCheckOwnership = true) : IAuthenticator<MicrosoftAccount> {
    private readonly MicrosoftAccount _account;
    private readonly string _clientId = clientId;
    private OAuth2TokenResponse _oAuth2TokenResponse;
    private readonly IEnumerable<string> _scopes = ["XboxLive.signin", "offline_access", "openid", "profile", "email"];

    public bool IsCheckOwnership { get; set; } = isCheckOwnership;

    public MicrosoftAuthenticator(MicrosoftAccount account, string clientId, bool isCheckOwnership)
        : this(clientId, isCheckOwnership) {
        _account = account;
    }

    public MicrosoftAccount Authenticate() {
        var task = AuthenticateAsync();
        if (task is { IsFaulted: false, IsCompleted: true }) {
            return task.GetAwaiter().GetResult();
        }

        return null;
    }

    public async ValueTask<MicrosoftAccount> AuthenticateAsync() {
        /*
         * Refresh token
         */
        if (_account is not null) {
            var url = "https://login.live.com/oauth20_token.srf";

            var content = new {
                client_id = _clientId,
                refresh_token = _account.RefreshToken,
                grant_type = "refresh_token",
            };

            var result = await url.PostUrlEncodedAsync(content);
            _oAuth2TokenResponse = await result.GetJsonAsync<OAuth2TokenResponse>();
        }

        /*
         * Get Xbox live token
         */
        var xblContent = new {
            Properties = new {
                AuthMethod = "RPS",
                SiteName = "user.auth.xboxlive.com",
                RpsTicket = $"d={_oAuth2TokenResponse.AccessToken}"
            },
            RelyingParty = "http://auth.xboxlive.com",
            TokenType = "JWT"
        };

        using var xblJsonReq = await $"https://user.auth.xboxlive.com/user/authenticate"
            .PostJsonAsync(xblContent);

        var xblTokenNode = (await xblJsonReq.GetStringAsync())
            .AsNode();

        /*
         * Get Xbox security token service token
         */
        var xstsContent = new {
            Properties = new {
                SandboxId = "RETAIL",
                UserTokens = new[] {
                    xblTokenNode.GetString("Token")
                }
            },
            RelyingParty = "rp://api.minecraftservices.com/",
            TokenType = "JWT"
        };

        using var xstsJsonReq = await $"https://xsts.auth.xboxlive.com/xsts/authorize"
            .PostJsonAsync(xstsContent);

        var xstsTokenNode = (await xstsJsonReq.GetStringAsync())
            .AsNode();

        /*
         * Authenticate minecraft account
         */
        var authenticateMinecraftContent = new {
            identityToken = $"XBL3.0 x={xblTokenNode["DisplayClaims"]["xui"].AsArray()
                .FirstOrDefault()
                .GetString("uhs")};{xstsTokenNode!
                .GetString("Token")}"
        };

        using var authenticateMinecraftPostRes = await $"https://api.minecraftservices.com/authentication/login_with_xbox"
            .PostJsonAsync(authenticateMinecraftContent);

        string access_token = (await authenticateMinecraftPostRes
                .GetStringAsync())
            .AsNode()
            .GetString("access_token");

        /*
         * Check player's minecraft ownership (optional steps)
         */
        if (IsCheckOwnership) {
            using var gameHasRes = await "https://api.minecraftservices.com/entitlements/mcstore"
                .WithHeader("Authorization", $"Bearer {access_token}")
                .GetAsync();

            var ownNode = (await gameHasRes.GetStringAsync())
                .AsNode();
            if (!ownNode["items"].AsArray().Any()) {
                throw new OperationCanceledException("Game not purchased, login terminated");
            }
        }

        /*
         * Get player's minecraft profile
         */
        using var profileRes = await "https://api.minecraftservices.com/minecraft/profile"
            .WithHeader("Authorization", $"Bearer {access_token}")
            .GetAsync();

        var profileNode = (await profileRes.GetStringAsync())
            .AsNode();

        string refreshToken = _oAuth2TokenResponse is null && string
            .IsNullOrEmpty(_oAuth2TokenResponse.RefreshToken)
            ? "None"
            : _oAuth2TokenResponse.RefreshToken;

        return new MicrosoftAccount {
            AccessToken = access_token,
            Type = AccountType.Microsoft,
            Name = profileNode.GetString("name"),
            Uuid = Guid.Parse(profileNode.GetString("id")),
            RefreshToken = refreshToken
        };
    }

    public async Task<OAuth2TokenResponse> DeviceFlowAuthAsync(Action<DeviceCodeResponse> deviceCode,
        CancellationTokenSource source = default) {
        if (string.IsNullOrEmpty(_clientId)) {
            throw new ArgumentNullException("ClientId is empty!");
        }

        var token = source?.Token;
        string tenant = "/consumers";
        var parameters = new Dictionary<string, string> {
            ["client_id"] = _clientId,
            ["tenant"] = tenant,
            ["scope"] = string.Join(" ", _scopes)
        };

        string json = await "https://login.microsoftonline.com/consumers/oauth2/v2.0/devicecode"
            .PostUrlEncodedAsync(parameters)
            .ReceiveString();

        var codeResponse = json.Deserialize(DeviceCodeResponseContext.Default.DeviceCodeResponse);
        deviceCode.Invoke(codeResponse);

        //Polling
        TimeSpan pollingInterval = TimeSpan.FromSeconds(codeResponse.Interval);
        DateTimeOffset codeExpiresOn = DateTimeOffset.UtcNow.AddSeconds(codeResponse.ExpiresIn);
        TimeSpan timeRemaining = codeExpiresOn - DateTimeOffset.UtcNow;
        OAuth2TokenResponse tokenResponse = default!;

        while (timeRemaining.TotalSeconds > 0) {
            if (token.HasValue && token.Value.IsCancellationRequested) {
                break;
            }

            parameters = new Dictionary<string, string> {
                ["grant_type"] = "urn:ietf:params:oauth:grant-type:device_code",
                ["device_code"] = codeResponse.DeviceCode,
                ["client_id"] = _clientId,
                ["tenant"] = tenant
            };

            string tokenJson = await "https://login.microsoftonline.com/consumers/oauth2/v2.0/token"
                .PostUrlEncodedAsync(new FormUrlEncodedContent(parameters))
                .ReceiveString();

            var tempTokenResponse = tokenJson.AsNode();

            if (tempTokenResponse["error"] == null) {
                tokenResponse = new() {
                    AccessToken = tempTokenResponse.GetString("access_token"),
                    RefreshToken = tempTokenResponse.GetString("refresh_token"),
                    ExpiresIn = tempTokenResponse.GetInt32("expires_in"),
                };
            }

            if (tempTokenResponse.GetString("token_type") is "Bearer") {
                _oAuth2TokenResponse = tokenResponse;
                return tokenResponse;
            }

            await Task.Delay(pollingInterval);
            timeRemaining = codeExpiresOn - DateTimeOffset.UtcNow;
        }

        throw new TimeoutException("登录操作已超时");
    }
}