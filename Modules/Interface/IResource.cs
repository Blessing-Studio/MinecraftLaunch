using MinecraftLaunch.Modules.Models.Download;

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
