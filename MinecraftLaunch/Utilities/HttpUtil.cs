using Flurl.Http;

namespace MinecraftLaunch.Utilities;

public static class HttpUtil {
    public static IFlurlClient FlurlClient { get; private set; }

    public static IFlurlClient Initialize(IFlurlClient client = default) {
        if (client is not null)
            return FlurlClient = client;

        return FlurlClient = new FlurlClient {
            Settings = {
                Timeout = TimeSpan.FromSeconds(100),
            }
        }.WithHeader("User-Agent", "MinecraftLaunch/1.0");
    }
}