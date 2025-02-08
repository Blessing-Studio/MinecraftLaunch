using MinecraftLaunch.Base.Models.Authentication;

namespace MinecraftLaunch.Base.Models.Game;

public record LaunchConfig {
    public Account Account { get; set; }

    public bool IsFullscreen { get; set; }
    public bool IsEnableIndependencyCore { get; set; } = true;

    public int MinMemorySize { get; set; }
    public int MaxMemorySize { get; set; } = 1024;

    public JavaEntry JavaPath { get; set; }
    public string LauncherName { get; set; }
    public string NativesFolder { get; set; }

    public IEnumerable<string> JvmArguments { get; set; }
}