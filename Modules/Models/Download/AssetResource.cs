using System.IO;
using MinecraftLaunch.Modules.Interface;
using MinecraftLaunch.Modules.Utilities;

namespace MinecraftLaunch.Modules.Models.Download;

public class AssetResource : IResource {
    public DirectoryInfo Root { get; set; }

    public string Name { get; set; }

    public int Size { get; set; }

    public string CheckSum { get; set; }

    public FileInfo ToFileInfo() {
        return new FileInfo(Path.Combine(Root.FullName, "assets", "objects", CheckSum.Substring(0, 2), CheckSum));
    }

    public DownloadRequest ToDownloadRequest() {
        DownloadRequest val = new();
        val.Directory = ToFileInfo().Directory!;
        val.FileName = CheckSum;
        val.FileSize = Size;
        val.Url = ExtendUtil.Combine(new string[3]
        {
            APIManager.Current.Assets,
            CheckSum.Substring(0, 2),
            CheckSum
        });
        return val;
    }
}
