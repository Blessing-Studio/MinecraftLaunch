using MinecraftLaunch.Classes.Enums;
using MinecraftLaunch.Classes.Interfaces;

namespace MinecraftLaunch.Classes.Models.Game;

public sealed class AssetEntry : IDownloadEntry {
    public int Size { get; set; }

    public string Name { get; set; }

    public bool IsNative { get; set; }

    public string Checksum { get; set; }

    public required string Url { get; set; }

    public required string Path { get; set; }

    public required string RelativePath { get; set; }

    public DownloadEntryType Type => DownloadEntryType.Asset;
}