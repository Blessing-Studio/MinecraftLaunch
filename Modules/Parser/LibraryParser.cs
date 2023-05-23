using System.Collections.Generic;
using System.IO;
using MinecraftLaunch.Modules.Models.Download;
using MinecraftLaunch.Modules.Models.Launch;
using MinecraftLaunch.Modules.Toolkits;

namespace MinecraftLaunch.Modules.Parser;

public class LibraryParser
{
	public List<LibraryJsonEntity> Entities { get; set; }

	public DirectoryInfo Root { get; set; }

	public LibraryParser(List<LibraryJsonEntity> entities, DirectoryInfo root)
	{
		Entities = entities;
		Root = root;
	}

	public IEnumerable<LibraryResource> GetLibraries()
	{
		foreach (LibraryJsonEntity libraryJsonEntity in Entities)
		{
			LibraryResource obj = new LibraryResource
			{
				CheckSum = (libraryJsonEntity.Downloads?.Artifact?.Sha1 ?? string.Empty)
			};
			DownloadsJsonEntity downloads = libraryJsonEntity.Downloads;
			int num;
			if (downloads == null)
			{
				num = 1;
			}
			else
			{
				FileJsonEntity artifact = downloads.Artifact;
				if (artifact == null)
				{
					num = 1;
				}
				else
				{
					_ = artifact.Size;
					num = 0;
				}
			}
			obj.Size = ((num == 0) ? (libraryJsonEntity.Downloads?.Artifact?.Size).Value : 0);
			obj.Url = (libraryJsonEntity.Downloads?.Artifact?.Url ?? string.Empty) + libraryJsonEntity.Url;
			obj.Name = libraryJsonEntity.Name;
			obj.Root = Root;
			obj.IsEnable = true;
			LibraryResource libraryResource = obj;
			if (libraryJsonEntity.Rules != null)
			{
				libraryResource.IsEnable = GetAblility(libraryJsonEntity, EnvironmentToolkit.GetPlatformName());
			}
			if (libraryJsonEntity.Natives != null)
			{
				libraryResource.IsNatives = true;
				if (!libraryJsonEntity.Natives.ContainsKey(EnvironmentToolkit.GetPlatformName()))
				{
					libraryResource.IsEnable = false;
				}
				if (libraryResource.IsEnable)
				{
					libraryResource.Name = libraryResource.Name + ":" + GetNativeName(libraryJsonEntity);
					FileJsonEntity file = libraryJsonEntity.Downloads.Classifiers[libraryJsonEntity.Natives[EnvironmentToolkit.GetPlatformName()].Replace("${arch}", EnvironmentToolkit.Arch)];
					libraryResource.CheckSum = file.Sha1;
					libraryResource.Size = file.Size;
					libraryResource.Url = file.Url;
				}
			}
			yield return libraryResource;
		}
	}

	private string GetNativeName(LibraryJsonEntity libraryJsonEntity)
	{
		return libraryJsonEntity.Natives[EnvironmentToolkit.GetPlatformName()].Replace("${arch}", EnvironmentToolkit.Arch);
	}

	private bool GetAblility(LibraryJsonEntity libraryJsonEntity, string platform)
	{
		bool linux;
		bool osx;
		bool windows = (linux = (osx = false));
		foreach (RuleEntity item in libraryJsonEntity.Rules)
		{
			if (item.Action == "allow")
			{
				if (item.System == null)
				{
					windows = (linux = (osx = true));
					continue;
				}
				using Dictionary<string, string>.Enumerator enumerator2 = item.System.GetEnumerator();
				while (enumerator2.MoveNext())
				{
					switch (enumerator2.Current.Value)
					{
					case "windows":
						windows = true;
						break;
					case "linux":
						linux = true;
						break;
					case "osx":
						osx = true;
						break;
					}
				}
			}
			else
			{
				if (!(item.Action == "disallow"))
				{
					continue;
				}
				if (item.System == null)
				{
					windows = (linux = (osx = false));
					continue;
				}
				using Dictionary<string, string>.Enumerator enumerator2 = item.System.GetEnumerator();
				while (enumerator2.MoveNext())
				{
					switch (enumerator2.Current.Value)
					{
					case "windows":
						windows = false;
						break;
					case "linux":
						linux = false;
						break;
					case "osx":
						osx = false;
						break;
					}
				}
			}
		}
		return platform switch
		{
			"windows" => windows, 
			"linux" => linux, 
			"osx" => osx, 
			_ => false, 
		};
	}
}
