using System.IO;
using Natsurainko.Toolkits.Network.Model;

namespace MinecraftLaunch.Modules.Interface;

public interface IResource
{
	DirectoryInfo Root { get; set; }

	string Name { get; }

	int Size { get; }

	string CheckSum { get; }

	FileInfo ToFileInfo();

	HttpDownloadRequest ToDownloadRequest();
}
