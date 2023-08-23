using MinecraftLaunch.Modules.Models.Auth;

namespace MinecraftLaunch.Modules.Models.Launch;

public class LaunchConfig {
    public Account Account { get; set; }

    public JvmConfig JvmConfig { get; set; }

    public bool IsServer { get; set; } = true;

    public bool IsChinese { get; set; } = false;

    public ServerConfig ServerConfig { get; set; }

    public DirectoryInfo? NativesFolder { get; set; }

    public string LauncherName { get; set; } = "release";

    public DirectoryInfo WorkingFolder { get; private set; }

    public bool IsEnableIndependencyCore { get; set; } = true;

    public GameWindowConfig GameWindowConfig { get; set; } = new();

    public LaunchConfig() {
    }

    public LaunchConfig(Account account) {
        Account = account;
    }

    public LaunchConfig(Account account, JvmConfig jvmConfig) {
        Account = account;
        JvmConfig = jvmConfig;
    }

    public LaunchConfig(Account account, JvmConfig jvmConfig, GameWindowConfig gameWindowConfig) {
        Account = account;
        JvmConfig = jvmConfig;
        GameWindowConfig = gameWindowConfig;
    }

    public LaunchConfig(Account account, JvmConfig jvmConfig, GameWindowConfig gameWindowConfig, ServerConfig serverConfig) {
        Account = account;
        JvmConfig = jvmConfig;
        GameWindowConfig = gameWindowConfig;
        ServerConfig = serverConfig;
    }
}
