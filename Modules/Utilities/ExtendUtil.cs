using System.Net;
using MinecraftLaunch.Modules.Enum;
using MinecraftLaunch.Modules.Installer;
using MinecraftLaunch.Modules.Interface;
using MinecraftLaunch.Modules.Models.Download;
using MinecraftLaunch.Modules.Models.Launch;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.IO.Compression;
using System.Security.Cryptography;

namespace MinecraftLaunch.Modules.Utilities;

public static class ExtendUtil {
    public static string ToPath(this string raw) {
        if (!Enumerable.Contains(raw, ' ')) {
            return raw;
        }
        return "\"" + raw + "\"";
    }

    public static string Replace(this string raw, Dictionary<string, string> keyValuePairs) {
        string text = raw;
        foreach (KeyValuePair<string, string> keyValuePair in keyValuePairs) {
            text = text.Replace(keyValuePair.Key, keyValuePair.Value);
        }
        return text;
    }

    public static void DeleteAllFiles(this DirectoryInfo directory) {
        foreach (FileInfo item in directory.EnumerateFiles()) {
            try {
                item.Delete();
            }
            catch (UnauthorizedAccessException) {
            }
        }
        foreach (DirectoryInfo item2 in directory.EnumerateDirectories()) {
            try {
                item2.DeleteAllFiles();
                item2.Delete();
            }
            catch (UnauthorizedAccessException) {
                Console.WriteLine("6S");
            }
        }
    }

    public static T ToJsonEntity<T>(this T entity, string json) where T : IJsonEntity {
        var options = new JsonSerializerOptions() {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true,
        };
        options.Converters.Add(new DateTimeConverter());

        return JsonSerializer.Deserialize<T>(json, options:options)!;
    }

    public static string ToJson<T>(this T entity) where T : IJsonEntity {
        var options = new JsonSerializerOptions() {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true,
        };
        options.Converters.Add(new DateTimeConverter());

        return JsonSerializer.Serialize(entity, options);
    }

    public static string ToJson(this object entity) {
        var options = new JsonSerializerOptions() {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true,
        };
        options.Converters.Add(new DateTimeConverter());

        return JsonSerializer.Serialize(entity, options);
    }

    public static T FromJson<T>(this T entity, string json) where T : IJsonEntity {
        return JsonSerializer.Deserialize<T>(json)!;
    }

    public static T ToJsonEntity<T>(this string json) {
        return JsonSerializer.Deserialize<T>(json)!;
    }

    public static string ToDownloadLink(this OpenJdkType open, JdkDownloadSource jdkDownloadSource) {
        foreach (KeyValuePair<string, KeyValuePair<string, string>[]> i in JavaInstaller.OpenJdkDownloadSourcesForWindows) {
            if (i.Key == "OpenJDK 8" && open == OpenJdkType.OpenJdk8) {
                return i.Value[0].Value;
            }
            if (i.Key == "OpenJDK 11" && open == OpenJdkType.OpenJdk11) {
                if (jdkDownloadSource == JdkDownloadSource.JdkJavaNet) {
                    return i.Value[0].Value;
                }
                return i.Value[1].Value;
            }
            if (i.Key == "OpenJDK 17" && open == OpenJdkType.OpenJdk17) {
                if (jdkDownloadSource == JdkDownloadSource.JdkJavaNet) {
                    return i.Value[0].Value;
                }
                return i.Value[1].Value;
            }
            if (i.Key == "OpenJDK 18" && open == OpenJdkType.OpenJdk18) {
                return i.Value[0].Value;
            }
        }
        return "";
    }

    public static string ToFullJavaPath(this OpenJdkType open, string Save) {
        string javapath = null;
        switch (open) {
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

    public static List<ModrinthFileInfo> GetModInfoToVersion(this List<ModrinthProjectInfoItem> ms, string version) {
        string version2 = version;
        List<ModrinthFileInfo> result = new List<ModrinthFileInfo>();
        ms.ForEach(delegate (ModrinthProjectInfoItem m) {
            ModrinthProjectInfoItem i = m;
            i.GameVersion.ForEach(delegate (string x) {
                if (x.Equals(version2)) {
                    i.Files.ForEach(delegate (ModrinthFileInfo a) {
                        result.Add(a);
                    });
                }
            });
        });
        return result;
    }

    public static async ValueTask<HttpDownloadResponse> InstallLatestVersion(this List<CurseForgeModpackFileInfo> raw, string folder) {
        HttpDownloadResponse res = await HttpUtil.HttpDownloadAsync(raw.First().DownloadUrl, folder);
        if (res.HttpStatusCode == HttpStatusCode.OK) {
            return res;
        }

        throw new WebException("下载失败");
    }

    public static bool IsDirectory(this string path) => Directory.Exists(path);

    public static bool IsDirectory(this string path, bool isCreate) {
        if (isCreate) {
            Directory.CreateDirectory(path);
        }

        return Directory.Exists(path);
    }

    public static bool IsDirectory(this DirectoryInfo path) => path!.Exists;

    public static bool IsFile(this string path) => File.Exists(path);

    public static bool IsFile(this FileInfo path) => path!.Exists;

    public static DirectoryInfo[] FindAllDirectory(this string path) => Directory.GetDirectories(path).Select(x => new DirectoryInfo(x)).ToArray();

    public static FileInfo[] FindAllFile(this string path) => Directory.GetFiles(path).Select(x => new FileInfo(x)).ToArray();

    public static string GetVersionsPath(this GameCore row) => Path.Combine(row.Root!.FullName, "versions");

    public static string GetModsPath(this GameCore row, bool Isolate = true) => Path.Combine(row.Root!.FullName, Isolate ? Path.Combine("versions", row.Id) : "", "mods");

    public static string GetGameCorePath(this GameCore row, bool Isolate = true) => Path.Combine(Isolate ? row.GetVersionsPath() : row.Root.FullName, Isolate ? row.Id! : string.Empty);

    public static string GetOptionsFilePath(this GameCore row, bool Isolate = true) => Path.Combine(GetGameCorePath(row, Isolate), "options.txt");

    public static string GetResourcePacksPath(this GameCore row, bool Isolate = true) => Path.Combine(row.Root!.FullName, Isolate ? Path.Combine("versions", row.Id) : "", "resourcepacks");

    public static string Join(this HttpDownloadRequest request) {
        return Path.Combine(request.Directory.FullName, request.FileName);
    }

    public static string LengthToMb(this long value) {
        return $"{(double)value / 1048576.0:0.0} Mb";
    }

    public static IEnumerable<(long, long)> SplitIntoRange(this long value, int rangeCount) {
        long add;
        for (long a = 0L; value > a; a += add) {
            add = value / rangeCount;
            if (a + add > value) {
                add = value - a;
            }

            yield return (a, a + add);
        }
    }

    public static string Combine(params string[] paths) {
        return string.Join("/", paths);
    }

    public static string GetString(this ZipArchiveEntry zipArchiveEntry) {
        using Stream stream = zipArchiveEntry.Open();
        using StreamReader streamReader = new StreamReader(stream);
        return streamReader.ReadToEnd();
    }

    public static void ExtractTo(this ZipArchiveEntry zipArchiveEntry, string filename) {
        FileInfo fileInfo = new FileInfo(filename);
        if (!fileInfo.Directory!.Exists) {
            fileInfo.Directory!.Create();
        }

        zipArchiveEntry.ExtractToFile(filename, overwrite: true);
    }

    public static bool Verify(this FileInfo file, int size) {
        return file.Exists && file.Length == size;
    }
     

    public static bool Verify(this FileInfo file, string sha1) {
        if (!file.Exists)
            return false;

        try {
            using var fileStream = File.OpenRead(file.FullName);
            using var provider = new SHA1CryptoServiceProvider();
            byte[] bytes = provider.ComputeHash(fileStream);

            return sha1.ToLower() == BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }
        catch {
            return false;
        }
    }
}
