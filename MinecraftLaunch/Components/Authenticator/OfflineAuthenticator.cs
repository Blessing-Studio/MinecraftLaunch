using System.Text;
using System.Security.Cryptography;
using MinecraftLaunch.Classes.Interfaces;
using MinecraftLaunch.Classes.Models.Auth;

namespace MinecraftLaunch.Components.Authenticator;

/// <summary>
/// Authenticator for offline accounts.
/// </summary>
/// <param name="name">The name of the account.</param>
/// <param name="uuid">The UUID of the account. If not provided, a new UUID will be generated based on the account name.</param>
public sealed class OfflineAuthenticator(string name, Guid? uuid = default) : IAuthenticator<OfflineAccount> {
    /// <summary>
    /// Authenticates the offline account.
    /// </summary>
    /// <returns>The authenticated offline account.</returns>
    public OfflineAccount Authenticate() {
        return new() {
            AccessToken = Guid.NewGuid().ToString("N"),
            Name = name,
            Uuid = uuid ?? new(MD5.HashData(Encoding.UTF8.GetBytes(name)))
        };
    }
}