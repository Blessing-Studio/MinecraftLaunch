namespace MinecraftLaunch.Classes.Models.Game;

public sealed record JavaEntry {
    public bool Is64Bit { get; init; }

    public string JavaPath { get; init; }

    public string JavaVersion { get; init; }

    public int JavaSlugVersion { get; init; }

    public string JavaDirectoryPath { get; init; }
}