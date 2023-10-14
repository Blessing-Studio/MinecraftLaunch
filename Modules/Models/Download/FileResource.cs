using System.IO;
using MinecraftLaunch.Modules.Interface;

namespace MinecraftLaunch.Modules.Models.Download;

public class FileResource : IResource {
    public DirectoryInfo? Root { get; set; }

    public string? Name { get; set; }

    public int Size { get; set; }

    public string? CheckSum { get; set; }

    public string? Url { get; set; }

    public FileInfo? FileInfo { get; set; }

    public DownloadRequest ToDownloadRequest() {
        return new DownloadRequest {
            Directory = FileInfo.Directory,
            FileName = Name,
            FileSize = Size,
            Url = Url
        };
    }

    public FileInfo? ToFileInfo() {
        return FileInfo;
    }
}
