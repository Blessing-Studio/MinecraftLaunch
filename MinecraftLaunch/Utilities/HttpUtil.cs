using Flurl.Http;

namespace MinecraftLaunch.Utilities;

public static class HttpUtil {
    public static FlurlClient FlurlClient { get; private set; }

    public static void Initialize() {
        FlurlClient = new FlurlClient {
            Settings = {
                Timeout = TimeSpan.FromSeconds(100),
            }
        }.WithHeader("User-Agent", "MinecraftLaunch/1.0");
    }
}