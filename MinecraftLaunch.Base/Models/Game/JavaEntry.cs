namespace MinecraftLaunch.Base.Models.Game;

public record JavaEntry {
    public bool Is64bit { get; init; }
    public string JavaPath { get; init; }
    public string JavaType { get; init; }
    public string JavaVersion { get; init; }

    public string JavaFolder => Path.GetDirectoryName(JavaPath);
    public int MajorVersion => Convert.ToInt32(JavaVersion.Split('.').FirstOrDefault());
}