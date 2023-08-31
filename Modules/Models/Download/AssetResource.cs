using System.IO;
using MinecraftLaunch.Modules.Interface;
using MinecraftLaunch.Modules.Utils;

namespace MinecraftLaunch.Modules.Models.Download;

public class AssetResource : IResource {
    public DirectoryInfo Root { get; set; }

    public string Name { get; set; }

    public int Size { get; set; }

    public string CheckSum { get; set; }

    public FileInfo ToFileInfo() {
        return new FileInfo(Path.Combine(Root.FullName, "assets", "objects", CheckSum.Substring(0, 2), CheckSum));
    }

    public HttpDownloadRequest ToDownloadRequest() {
        HttpDownloadRequest val = new HttpDownloadRequest();
        val.Directory = ToFileInfo().Directory;
        val.FileName = CheckSum;
        val.Sha1 = CheckSum;
        val.Size = Size;
        val.Url = ExtendUtil.Combine(new string[3]
        {
            APIManager.Current.Assets,
            CheckSum.Substring(0, 2),
            CheckSum
        });
        return val;
    }
}
