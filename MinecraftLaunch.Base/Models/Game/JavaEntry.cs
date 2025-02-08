namespace MinecraftLaunch.Base.Models.Game;

public record JavaEntry {
    public bool Is64bit { get; init; }
    public string JavaPath { get; init; }
    public string JavaType { get; init; }
    public Version JavaVersion { get; init; }

    public string JavaFolder => Path.GetDirectoryName(JavaPath);
    public int MajorVersion => JavaVersion.Major is 1 ? JavaVersion.Minor : JavaVersion.Major;
}