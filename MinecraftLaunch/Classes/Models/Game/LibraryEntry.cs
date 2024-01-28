using MinecraftLaunch.Classes.Enums;
using MinecraftLaunch.Classes.Interfaces;

namespace MinecraftLaunch.Classes.Models.Game;

public sealed record LibraryEntry : IDownloadEntry {
    public int Size { get; set; }

    public string Url { get; set; }

    public bool IsNative { get; set; }

    public string Checksum { get; set; }

    public string Path { get; set; }

    public string RelativePath { get; set; }

    public DownloadEntryType Type => DownloadEntryType.Library;
}