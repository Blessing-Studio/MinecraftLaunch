using MinecraftLaunch.Classes.Interfaces;
using MinecraftLaunch.Classes.Models.Auth;

namespace MinecraftLaunch.Components.Authenticator {//W.I.P
    public class MicrosoftAuthenticator : IAuthenticator<MicrosoftAccount> {
        public MicrosoftAccount Authenticate() {
            return AuthenticateAsync().GetAwaiter().GetResult();
        }

        public ValueTask<MicrosoftAccount> AuthenticateAsync() {
            throw new NotImplementedException();
        }
    }
}
