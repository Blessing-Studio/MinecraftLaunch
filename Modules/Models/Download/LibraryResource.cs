using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using MinecraftLaunch.Modules.Interface;
using MinecraftLaunch.Modules.Toolkits;
using Natsurainko.Toolkits.Network;
using Natsurainko.Toolkits.Network.Model;

namespace MinecraftLaunch.Modules.Models.Download;

public class LibraryResource : IResource
{
	public DirectoryInfo Root { get; set; }

	public string Name { get; set; }

	public bool IsEnable { get; set; }

	public bool IsNatives { get; set; }

	public int Size { get; set; }

	public string CheckSum { get; set; }

	public string Url { get; set; }

	public FileInfo ToFileInfo()
	{
		string root = Path.Combine(Root.FullName, "libraries");
		foreach (string item in FormatName(Name))
		{
			root = Path.Combine(root, item);
		}
		Trace.WriteLine(root);
		return new FileInfo(root);
	}

	public static IEnumerable<string> FormatName(string Name)
	{
		string[] extension = (Name.Contains("@") ? Name.Split('@') : Array.Empty<string>());
		string[] subString = (extension.Any() ? Name.Replace("@" + extension[1], string.Empty).Split(':') : Name.Split(':'));
		string[] array = subString[0].Split('.');
		for (int i = 0; i < array.Length; i++)
		{
			yield return array[i];
		}
		yield return subString[1];
		yield return subString[2];
		if (!extension.Any())
		{
			yield return $"{subString[1]}-{subString[2]}{((subString.Length > 3) ? ("-" + subString[3]) : string.Empty)}.jar";
		}
		else
		{
			yield return $"{subString[1]}-{subString[2]}{((subString.Length > 3) ? ("-" + subString[3]) : string.Empty)}.jar".Replace("jar", extension[1]);
		}
	}

	public HttpDownloadRequest ToDownloadRequest()
	{
		string root = APIManager.Current.Libraries;
		foreach (string item in FormatName(Name))
		{
			root = UrlExtension.Combine(new string[2] { root, item });
		}
		if (!string.IsNullOrEmpty(Url) && (!Url.Contains("fabricmc") || !Url.Contains("quiltmc") || !Url.Contains("minecraftforge")))
		{
			root = (APIManager.Current.Host.Equals(APIManager.Mojang.Host) ? Url : Url.Replace(APIManager.Mojang.Libraries, APIManager.Current.Libraries).Replace(APIManager.ForgeLibraryUrlReplace).Replace(APIManager.FabricLibraryUrlReplace));
		}
		return new HttpDownloadRequest
		{
			Directory = ToFileInfo().Directory,
			FileName = ToFileInfo().Name,
			Sha1 = CheckSum,
			Size = Size,
			Url = root
		};
	}
}
