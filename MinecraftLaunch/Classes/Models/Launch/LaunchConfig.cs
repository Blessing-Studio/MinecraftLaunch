using MinecraftLaunch.Classes.Models.Auth;

namespace MinecraftLaunch.Classes.Models.Launch;

public sealed record LaunchConfig() {
    public Account Account { get; set; }

    public JvmConfig JvmConfig { get; set; }

    public ServerConfig ServerConfig { get; set; }

    public DirectoryInfo? NativesFolder { get; set; }

    public string LauncherName { get; set; } = "release";

    public DirectoryInfo WorkingFolder { get; private set; }

    public bool IsEnableIndependencyCore { get; set; } = true;

    public GameWindowConfig GameWindowConfig { get; set; } = new();
        
    public LaunchConfig(Account account) : this() {
        Account = account;
    }

    public LaunchConfig(Account account, JvmConfig jvmConfig) : this() {
        Account = account;
        JvmConfig = jvmConfig;
    }

    public LaunchConfig(Account account, JvmConfig jvmConfig, GameWindowConfig gameWindowConfig) : this() {
        Account = account;
        JvmConfig = jvmConfig;
        GameWindowConfig = gameWindowConfig;
    }

    public LaunchConfig(Account account, JvmConfig jvmConfig, GameWindowConfig gameWindowConfig, ServerConfig serverConfig) : this() {
        Account = account;
        JvmConfig = jvmConfig;
        GameWindowConfig = gameWindowConfig;
        ServerConfig = serverConfig;
    }
}

public sealed record JvmConfig(string file) {
    public FileInfo JavaPath { get; set; } = new(file);

    public int MaxMemory { get; set; } = 1024;

    public bool UsedGC { get; set; } = true;


    public int MinMemory { get; set; } = 512;

    public IEnumerable<string> AdvancedArguments { get; set; }

    public IEnumerable<string> GCArguments { get; set; }

    public static implicit operator JvmConfig(string file) => new(file);

    public static implicit operator JvmConfig(FileInfo fileInfo) => new(fileInfo);
}

public sealed record ServerConfig(int port, string ip) {
    public string Ip { get; set; } = ip;

    public int Port { get; set; } = port;
}

public sealed record GameWindowConfig {
    public int Width { get; set; } = 854;


    public int Height { get; set; } = 480;


    public bool IsFullscreen { get; set; }

    public static implicit operator GameWindowConfig(bool isFullscreen) => new() {
        IsFullscreen = isFullscreen,
    };
}