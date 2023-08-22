using System.IO;
using MinecraftLaunch.Modules.Models.Auth;

namespace MinecraftLaunch.Modules.Models.Launch;

public class LaunchConfig {
    public DirectoryInfo? NativesFolder { get; set; }

    public DirectoryInfo WorkingFolder { get; private set; }

    public Account Account { get; set; }

    public JvmConfig JvmConfig { get; set; }

    public ServerConfig ServerConfig { get; set; }

    public GameWindowConfig GameWindowConfig { get; set; } = new GameWindowConfig();

    public string LauncherName { get; set; } = "MinecraftLaunch";

    public bool IsServer { get; set; } = true;

    public bool IsEnableIndependencyCore { get; set; } = true;

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
