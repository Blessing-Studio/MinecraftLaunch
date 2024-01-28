using MinecraftLaunch.Classes.Enums;

namespace MinecraftLaunch.Classes.Models.Game;

public sealed record GameEntry {
    public string Id { get; set; }

    public string Version { get; set; }

    public string Type { get; set; }

    public int JavaVersion { get; set; }

    public LoaderType MainLoaderType { get; set; }

    public bool IsVanilla { get; set; }

    public bool IsInheritedFrom { get; set; }

    public GameEntry InheritsFrom { get; set; }

    public string GameFolderPath { get; set; }

    public string JarPath { get; set; }

    public string AssetsIndexJsonPath { get; set; }

    public string MainClass { get; set; }

    public IEnumerable<string> FrontArguments { get; set; }

    public IEnumerable<string> BehindArguments { get; set; }
}