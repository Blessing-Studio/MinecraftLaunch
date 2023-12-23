//
using MinecraftLaunch.Components.Authenticator;
using MinecraftLaunch.Components.Fetcher;
using MinecraftLaunch.Components.Launcher;
using MinecraftLaunch.Components.Resolver;

var resolver = new GameResolver("C:\\Users\\w\\Desktop\\temp\\.minecraft");
Launcher launcher = new(resolver, new(new OfflineAuthenticator("Yang114").Authenticate()) {
    JvmConfig = new(new JavaFetcher().Fetch().FirstOrDefault().JavaPath) {
        MaxMemory = 1024,
    }
});

await launcher.LaunchAsync("1.12.2");