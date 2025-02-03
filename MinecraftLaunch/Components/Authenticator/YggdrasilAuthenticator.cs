using Flurl.Http;
using MinecraftLaunch.Base.Models.Authentication;
using MinecraftLaunch.Extensions;
using System.Runtime.CompilerServices;

namespace MinecraftLaunch.Components.Authenticator;

public sealed class YggdrasilAuthenticator {
    private readonly string _url;
    private readonly string _email;
    private readonly string _password;

    /// <summary>
    /// Constructor for YggdrasilAuthenticator.
    /// </summary>
    /// <param name="url">The URL for authentication.</param>
    /// <param name="email">The email of the account.</param>
    /// <param name="password">The password of the account.</param>
    public YggdrasilAuthenticator(string url, string email, string password) {
        _url = url;
        _email = email;
        _password = password;
    }

    public async Task<YggdrasilAccount> RefreshAsync(YggdrasilAccount account, CancellationToken cancellationToken = default) {
        string url = _url;

        url += "/authserver/refresh";
        var payload = new {
            requestUser = true,
            clientToken = account.ClientToken,
            accessToken = account.AccessToken,
            selectedProfile = new {
                name = account.Name,
                id = account.Uuid.ToString("N")
            }
        };

        var json = await url.PostJsonAsync(payload, cancellationToken: cancellationToken)
            .ReceiveString();

        var entry = json.Deserialize(YggdrasilResponseContext.Default.YggdrasilResponse);
        var profile = entry.SelectedProfile;

        return new YggdrasilAccount(profile.Name, Guid.Parse(profile.Id), entry.AccessToken, _url, entry.ClientToken);
    }

    /// <summary>
    /// Asynchronously authenticates the Yggdrasil account.
    /// </summary>
    /// <returns>A ValueTask that represents the asynchronous operation. The task result contains the authenticated Yggdrasil account.</returns>
    public async IAsyncEnumerable<YggdrasilAccount> AuthenticateAsync([EnumeratorCancellation] CancellationToken cancellationToken = default) {
        string url = _url;

        url += "/authserver/authenticate";
        var payload = new {
            clientToken = Guid.NewGuid().ToString("N"),
            username = _email,
            password = _password,
            requestUser = false,
            agent = new {
                name = "Minecraft",
                version = 1
            }
        };

        var json = await url.PostJsonAsync(payload, cancellationToken: cancellationToken)
            .ReceiveString();

        var entry = json.Deserialize(YggdrasilResponseContext.Default.YggdrasilResponse);

        foreach (var profile in entry.AvailableProfiles) {
            yield return new YggdrasilAccount(profile.Name, Guid.Parse(profile.Id), entry.AccessToken, _url, entry.ClientToken);
        }
    }
}

//public class YggdrasilRefreshRequest {
//    [JsonPropertyName("accessToken")]
//    [JsonRequired]
//    public string AccessToken { get; set; } = null!;

//    [JsonPropertyName("clientToken")]
//    [JsonRequired]
//    public string ClientToken { get; set; } = null!;

//    [JsonPropertyName("requestUser")]
//    public bool RequestUser { get; set; } = true;

//    [JsonPropertyName("selectedProfile")]
//    public ProfileModel? SelectedProfile { get; set; }
//}