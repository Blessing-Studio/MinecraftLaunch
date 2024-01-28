using MinecraftLaunch.Classes.Enums;

namespace MinecraftLaunch.Classes.Interfaces;

public interface IDownloadEntry {
    int Size { get; set; }

    string Url { get; set; }

    string Path { get; set; }

    string Checksum { get; set; }

    DownloadEntryType Type { get; }
}