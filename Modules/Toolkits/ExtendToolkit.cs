using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using MinecraftLaunch.Modules.Enum;
using MinecraftLaunch.Modules.Installer;
using MinecraftLaunch.Modules.Interface;
using MinecraftLaunch.Modules.Models.Download;
using MinecraftLaunch.Modules.Models.Launch;
using Natsurainko.Toolkits.Network;
using Natsurainko.Toolkits.Network.Model;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Serialization;
using System.Text.Json;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace MinecraftLaunch.Modules.Toolkits;

public static class ExtendToolkit
{
	public static string ToPath(this string raw)
	{
		if (!Enumerable.Contains(raw, ' '))
		{
			return raw;
		}
		return "\"" + raw + "\"";
	}

	public static string Replace(this string raw, Dictionary<string, string> keyValuePairs)
	{
		string text = raw;
		foreach (KeyValuePair<string, string> keyValuePair in keyValuePairs)
		{
			text = text.Replace(keyValuePair.Key, keyValuePair.Value);
		}
		return text;
	}

	public static string Get(this string url)
	{
		return HttpWrapper.HttpGetAsync(url, (Tuple<string, string>)null, HttpCompletionOption.ResponseContentRead).Result.Content.ReadAsStringAsync().Result;
	}

	public static async ValueTask<string> Post(this string url, string content, Dictionary<string, string> keyValuePairs)
	{
		return await (await HttpWrapper.HttpPostAsync(url, content, keyValuePairs, "application/json")).Content.ReadAsStringAsync();
	}

	public static async ValueTask<string> Post(this string url, string content)
	{
		return await (await HttpWrapper.HttpPostAsync(url, content, "application/json")).Content.ReadAsStringAsync();
	}

	public static void DeleteAllFiles(this DirectoryInfo directory)
	{
		foreach (FileInfo item in directory.EnumerateFiles())
		{
			try
			{
				item.Delete();
			}
			catch (UnauthorizedAccessException)
			{
			}
		}
		foreach (DirectoryInfo item2 in directory.EnumerateDirectories())
		{
			try
			{
				item2.DeleteAllFiles();
				item2.Delete();
			}
			catch (UnauthorizedAccessException)
			{
			}
		}
	}

	public static T ToJsonEntity<T>(this T entity, string json) where T : IJsonEntity
	{
        return JsonSerializer.Deserialize<T>(json)!;
    }

    public static string ToJson<T>(this T entity) where T : IJsonEntity {	
        return JsonSerializer.Serialize((object)entity, options: new JsonSerializerOptions {		
			WriteIndented = true
		});
	}

	public static string ToJson(this object entity, bool IsIndented = true) {
	{
		if (IsIndented)		
			return JsonSerializer.Serialize(entity, options: new JsonSerializerOptions {    
				WriteIndented = true
            });
        }

		return JsonSerializer.Serialize(entity);
	}

	public static T FromJson<T>(this T entity, string json) where T : IJsonEntity
	{
        return JsonSerializer.Deserialize<T>(json)!;
    }

    public static T ToJsonEntity<T>(this string json)
	{
		return JsonSerializer.Deserialize<T>(json)!;
	}

    public static T ToNewtonJsonEntity<T>(this string json)
    {
        return JsonConvert.DeserializeObject<T>(json)!;
    }

    public static string ToDownloadLink(this OpenJdkType open, JdkDownloadSource jdkDownloadSource)
	{
		foreach (KeyValuePair<string, KeyValuePair<string, string>[]> i in JavaInstaller.OpenJdkDownloadSourcesForWindows)
		{
			if (i.Key == "OpenJDK 8" && open == OpenJdkType.OpenJdk8)
			{
				return i.Value[0].Value;
			}
			if (i.Key == "OpenJDK 11" && open == OpenJdkType.OpenJdk11)
			{
				if (jdkDownloadSource == JdkDownloadSource.JdkJavaNet)
				{
					return i.Value[0].Value;
				}
				return i.Value[1].Value;
			}
			if (i.Key == "OpenJDK 17" && open == OpenJdkType.OpenJdk17)
			{
				if (jdkDownloadSource == JdkDownloadSource.JdkJavaNet)
				{
					return i.Value[0].Value;
				}
				return i.Value[1].Value;
			}
			if (i.Key == "OpenJDK 18" && open == OpenJdkType.OpenJdk18)
			{
				return i.Value[0].Value;
			}
		}
		return "";
	}

	public static string ToFullJavaPath(this OpenJdkType open, string Save)
	{
		string javapath = null;
		switch (open)
		{
		case OpenJdkType.OpenJdk8:
			javapath += "OpenJDK 8";
			break;
		case OpenJdkType.OpenJdk11:
			javapath += "OpenJDK 11";
			break;
		case OpenJdkType.OpenJdk17:
			javapath += "OpenJDK 17";
			break;
		case OpenJdkType.OpenJdk18:
			javapath += "OpenJDK 18";
			break;
		}
		string obj = (Save.EndsWith('\\') ? (Save + javapath) : (Save.EndsWith("/") ? (Save + javapath) : (Save + "\\" + javapath)));
		Directory.CreateDirectory(obj);
		return obj;
	}

	public static List<ModrinthFileInfo> GetModInfoToVersion(this List<ModrinthProjectInfoItem> ms, string version)
	{
		string version2 = version;
		List<ModrinthFileInfo> result = new List<ModrinthFileInfo>();
		ms.ForEach(delegate(ModrinthProjectInfoItem m)
		{
			ModrinthProjectInfoItem i = m;
			i.GameVersion.ForEach(delegate(string x)
			{
				if (x.Equals(version2))
				{
					i.Files.ForEach(delegate(ModrinthFileInfo a)
					{
						result.Add(a);
					});
				}
			});
		});
		return result;
	}

	public static async ValueTask<HttpDownloadResponse> InstallLatestVersion(this List<CurseForgeModpackFileInfo> raw, string folder)
	{
		HttpDownloadResponse res = await HttpToolkit.HttpDownloadAsync(raw.First().DownloadUrl, folder);
		if (res.HttpStatusCode == HttpStatusCode.OK)
		{
			return res;
		}
		throw new WebException("下载失败");
	}

	public static bool IsDirectory(this string path) => Directory.Exists(path);

	public static bool IsDirectory(this DirectoryInfo path) => path!.Exists;

    public static bool IsFile(this string path) => File.Exists(path);

    public static bool IsFile(this FileInfo path) => path!.Exists;

	public static DirectoryInfo[] FindAllDirectory(this string path) => Directory.GetDirectories(path).Select(x => new DirectoryInfo(x)).ToArray();

    public static FileInfo[] FindAllFile(this string path) => Directory.GetFiles(path).Select(x => new FileInfo(x)).ToArray();

    public static string GetVersionsPath(this GameCore row) => Path.Combine(row.Root!.FullName, "versions");

    public static string GetModsPath(this GameCore row, bool Isolate = true) => Path.Combine(row.Root!.FullName, Isolate ? Path.Combine("versions", row.Id) : "", "mods");

	public static string GetGameCorePath(this GameCore row, bool Isolate = true) => Path.Combine(row.GetVersionsPath(), row.Id!);

    public static string GetResourcePacksPath(this GameCore row, bool Isolate = true) => Path.Combine(row.Root!.FullName, Isolate ? Path.Combine("versions", row.Id) : "", "resourcepacks");
}
