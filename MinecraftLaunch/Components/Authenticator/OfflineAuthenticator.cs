using MinecraftLaunch.Base.Models.Authentication;

namespace MinecraftLaunch.Components.Authenticator;

public sealed class OfflineAuthenticator {
    public OfflineAccount Authenticate(string name, Guid uuid = default) =>
        new(name, uuid, Guid.NewGuid().ToString());
}