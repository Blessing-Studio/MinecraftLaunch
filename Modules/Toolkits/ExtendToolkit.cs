using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Flurl.Http;
using MinecraftLaunch.Modules.Enum;
using MinecraftLaunch.Modules.Installer;
using MinecraftLaunch.Modules.Interface;
using MinecraftLaunch.Modules.Models.Auth;
using MinecraftLaunch.Modules.Models.Download;
using MinecraftLaunch.Modules.Models.Launch;
using Natsurainko.Toolkits.Network;
using Natsurainko.Toolkits.Network.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace MinecraftLaunch.Modules.Toolkits;

public static class ExtendToolkit
{
    const string NameChangeAPI = "https://api.minecraftservices.com/minecraft/profile/name/";

    const string UploadSkinAPI = "https://api.minecraftservices.com/minecraft/profile/skins";

    const string ResetSkinAPI = "https://api.minecraftservices.com/minecraft/profile/skins/active";

    const string CapeAPI = "https://api.minecraftservices.com/minecraft/profile/capes/active";

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
		return JsonConvert.DeserializeObject<T>(json);
	}

	public static string ToJson<T>(this T entity) where T : IJsonEntity
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		return JsonConvert.SerializeObject((object)entity, new JsonSerializerSettings
		{
			ContractResolver = (IContractResolver)new CamelCasePropertyNamesContractResolver(),
			Formatting = (Formatting)1,
			NullValueHandling = (NullValueHandling)1
		});
	}

	public static string ToJson(this object entity, bool IsIndented = true)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		if (IsIndented)
		{
			return JsonConvert.SerializeObject(entity, new JsonSerializerSettings
			{
				ContractResolver = (IContractResolver)new CamelCasePropertyNamesContractResolver(),
				Formatting = (Formatting)1,
				NullValueHandling = (NullValueHandling)1
			});
		}
		return JsonConvert.SerializeObject(entity);
	}

	public static T FromJson<T>(this T entity, string json) where T : IJsonEntity
	{
		return JsonConvert.DeserializeObject<T>(json);
	}

	public static T ToJsonEntity<T>(this string json)
	{
		return JsonConvert.DeserializeObject<T>(json);
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

    public static bool IsNull(this object obj) => obj is null;

    public static DirectoryInfo[] FindAllDirectory(this string path) => Directory.GetDirectories(path).Select(x => new DirectoryInfo(x)).ToArray();

    public static FileInfo[] FindAllFile(this string path) => Directory.GetFiles(path).Select(x => new FileInfo(x)).ToArray();

    public static string GetVersionsPath(this GameCore row) => Path.Combine(row.Root!.FullName, "versions");

    public static string GetModsPath(this GameCore row, bool Isolate = true) => Path.Combine(row.Root!.FullName, Isolate ? Path.Combine("versions", row.Id) : "", "mods");

	public static string GetGameCorePath(this GameCore row) => Path.Combine(row.GetVersionsPath(), row.Id!);

    public static string GetResourcePacksPath(this GameCore row, bool Isolate = true) => Path.Combine(row.Root!.FullName, Isolate ? Path.Combine("versions", row.Id) : "", "resourcepacks");

    public static string ToUri(this string raw)
    {
        if (raw.EndsWith('/'))
            return Regex.Replace(raw, ".$", "");
        else
            return raw;
    }

    public static async ValueTask<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> raw)
    {
        List<T> list = new List<T>();
        await foreach (var i in raw)
            list.Add(i);
        return list;
    }

    public static List<T> ToList<T>(this IAsyncEnumerable<T> raw) => ToListAsync(raw).Result;

    /// <summary>
    /// 异步上传皮肤至 Mojang 服务器
    /// </summary>
    /// <param name="path">皮肤文件地址</param>
    public static async ValueTask<bool> SkinUploadAsync(this MicrosoftAccount account, string path)
    {
        if (File.Exists(path))
        {
            var res = await UploadSkinAPI.AllowAnyHttpStatus()
                .WithOAuthBearerToken(account.AccessToken).PostMultipartAsync(content =>
                    content.AddString("variant", "slim")
                        .AddFile("file", path));

            var json = await res.GetStringAsync();

            if (!string.IsNullOrEmpty(json))
            {
                JObject model = new(json);
                if (account.Uuid.ToString() == model["id"]!.ToString() && account.Name == model["name"]!.ToString())
                    return true;
            }
        }
        else throw new Exception("错误：此皮肤路径不存在！");
        return false;
    }

    /// <summary>
    /// 异步上传皮肤至 Mojang 服务器
    /// </summary>
    /// <param name="path">皮肤文件地址</param>
    public static async ValueTask<bool> SkinUploadAsync(this MicrosoftAccount account, FileInfo path)
    {
        var res = await UploadSkinAPI.AllowAnyHttpStatus()
            .WithOAuthBearerToken(account.AccessToken).PostMultipartAsync(content =>
                content.AddString("variant", "slim")
                    .AddFile("file", path.FullName));

        var json = await res.GetStringAsync();

        if (!string.IsNullOrEmpty(json))
        {
            JObject model = new(json);
            if (account.Uuid.ToString() == model["id"]!.ToString() && account.Name == model["name"]!.ToString())
                return true;
        }

        return false;
    }

    /// <summary>
    /// 删除当前启用的皮肤并重置为默认皮肤
    /// </summary>
    /// <param name="account"></param>
    /// <returns></returns>
    public static async ValueTask<bool> SkinResetAsync(this MicrosoftAccount account)
    {
        var res = await ResetSkinAPI.AllowAnyHttpStatus()
        .WithOAuthBearerToken(account.AccessToken).DeleteAsync();

        var json = await res.GetStringAsync();
        if (!string.IsNullOrEmpty(json))
        {
            JObject model = new(json);
            if (account.Uuid.ToString() == model["id"]!.ToString() && account.Name == model["name"]!.ToString())
                return true;
        }
        return false;
    }

    /// <summary>
    /// 异步隐藏皮肤的披风（如果有）
    /// </summary>
    /// <param name="account"></param>
    /// <returns></returns>
    public static async ValueTask<bool> CapeHideAsync(this MicrosoftAccount account)
    {
        var res = await CapeAPI.AllowAnyHttpStatus().WithOAuthBearerToken(account.AccessToken).DeleteAsync();

        var json = await res.GetStringAsync();
        if (!string.IsNullOrEmpty(json))
        {
            JObject model = new(json);
            if (account.Uuid.ToString() == model["id"]!.ToString() && account.Name == model["name"]!.ToString())
                return true;
        }
        return false;
    }

    /// <summary>
    /// 异步显示隐藏的披风（如果有）
    /// </summary>
    /// <param name="capeId">披风 Id</param>
    /// <returns></returns>
    public static async ValueTask<bool> ShowCapeAsync(this MicrosoftAccount account, string capeId)
    {
        var content = new
        {
            capeId = capeId
        };
        var res = await CapeAPI.AllowAnyHttpStatus().WithOAuthBearerToken(account.AccessToken).PutJsonAsync(content);

        var json = await res.GetStringAsync();
        if (!string.IsNullOrEmpty(json))
        {
            JObject model = new(json);
            if (account.Uuid.ToString() == model["id"]!.ToString() && account.Name == model["name"]!.ToString())
                return true;
        }
        return false;
    }

    /// <summary>
    /// 异步更改游戏内用户名
    /// </summary>
    /// <param name="newName">新的用户名</param>
    /// <returns></returns>
    public static async ValueTask<bool> UsernameChangeAsync(this MicrosoftAccount account, string newName)
    {
        var fullUrl = $"{NameChangeAPI}{newName}";
        var code = (await fullUrl.WithOAuthBearerToken(account.AccessToken).PutAsync()).StatusCode;

        if (code is 200)
            return true;

        return false;
    }

    /// <summary>
    /// 检查是否可以更改为某个名称
    /// </summary>
    /// <param name="account"></param>
    /// <returns></returns>
    public static async ValueTask<bool> CheckNameIsUsableAsync(this MicrosoftAccount account, string newName)
    {
        var fullUrl = $"{NameChangeAPI}{newName}/available";
        var res = (await fullUrl.WithOAuthBearerToken(account.AccessToken).GetStringAsync());

        if (!string.IsNullOrEmpty(res))
        {
            JObject objects = new(res);
            if (objects["status"]!.ToString().Contains("NOT"))
                return false;
        }
        else return false;

        return true;
    }
}
