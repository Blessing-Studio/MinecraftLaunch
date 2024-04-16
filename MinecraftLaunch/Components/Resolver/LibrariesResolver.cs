using System.Text.Json;
using System.Text.Json.Nodes;
using MinecraftLaunch.Utilities;
using MinecraftLaunch.Extensions;
using MinecraftLaunch.Classes.Interfaces;
using MinecraftLaunch.Classes.Models.Game;
using MinecraftLaunch.Classes.Enums;
using System.Diagnostics;

namespace MinecraftLaunch.Components.Resolver;

/// <summary>
/// Minecraft 运行库解析器
/// </summary>
internal sealed class LibrariesResolver(GameEntry gameEntry) {
    private GameEntry GameEntry => gameEntry;

    public LibraryEntry Resolve(JsonNode libNode) {
        var jsonRules = libNode["rules"];
        var jsonNatives = libNode["natives"];

        if (jsonRules != null && !GetLibraryEnable(jsonRules.Deserialize<IEnumerable<RuleModel>>(
                JsonConverterUtil.DefaultJsonOptions)!)) {
            return null!;
        }

        var libraryEntry = libNode.Deserialize<LibraryJsonEntry>(JsonConverterUtil.DefaultJsonOptions);
        AppendNativeLibraryName(ref libraryEntry, jsonNatives);

        var relativePath = FormatLibraryNameToRelativePath(libraryEntry.Name);
        var path = Path.Combine(GameEntry.GameFolderPath, "libraries", relativePath);

        var librariesItem = new LibraryEntry {
            Path = path,
            RelativePath = relativePath,
            IsNative = jsonNatives != null,
        };

        GetLibraryChecksumAndUrl(ref librariesItem, libNode, libraryEntry);
        return librariesItem;
    }

    public IEnumerable<IDownloadEntry> GetLibraries() {
        var jsonPath = Path.Combine(GameEntry.OfVersionJsonPath());
        var libsNode = File.ReadAllText(jsonPath).AsNode()
            .GetEnumerable("libraries");

        foreach (var libNode in libsNode) {
            var lib = Resolve(libNode!);
            if (lib != null) {
                yield return lib;
            }
        }

        yield return GetJarEntry();

        if (GameEntry.IsInheritedFrom) {
            var temp = new LibrariesResolver(GameEntry.InheritsFrom);
            foreach (var lib in temp.GetLibraries()) {
                yield return lib;
            }
        }
    }

    public static IEnumerable<LibraryEntry> GetLibrariesFromJsonArray(JsonArray jsonArray, string path) {
        foreach (var libNode in jsonArray) {
            var libraryEntry = CreateLibraryEntryFromJsonNode(libNode, path);
            if (libraryEntry != null) {
                yield return libraryEntry;
            }
        }
    }

    public static string FormatLibraryNameToRelativePath(string name) {
        string path = string.Empty;
        foreach (var subPath in FormatLibraryName(name)) {
            path = Path.Combine(path, subPath);
        }

        return path;
    }

    private JarEntry GetJarEntry() {
        var jsonClient = File.ReadAllText(GameEntry
            .OfVersionJsonPath()).AsNode()?
            .Select("downloads")?
            .Select("client");

        if (jsonClient != null) {
            Debug.WriteLine(jsonClient.GetInt32("size"));
            return new JarEntry {
                Path = GameEntry.JarPath,
                McVersion = gameEntry.Version,
                Url = jsonClient.GetString("url"),
                Size = jsonClient.GetInt32("size"),
                Checksum = jsonClient.GetString("sha1"),
            };
        }

        return null;
    }

    private static IEnumerable<string> FormatLibraryName(string Name) {
        var extension = Name.Contains('@') ? Name.Split('@') : Array.Empty<string>();
        var subString = extension.Any()
            ? Name.Replace($"@{extension[1]}", string.Empty).Split(':')
            : Name.Split(':');

        foreach (string item in subString[0].Split('.'))
            yield return item;

        yield return subString[1];
        yield return subString[2];

        if (!extension.Any())
            yield return $"{subString[1]}-{subString[2]}{(subString.Length > 3 ? $"-{subString[3]}" : string.Empty)}.jar";
        else yield return $"{subString[1]}-{subString[2]}{(subString.Length > 3 ? $"-{subString[3]}" : string.Empty)}.jar".Replace("jar", extension[1]);
    }

    private bool GetLibraryEnable(IEnumerable<RuleModel> rules) {
        bool windows, linux, osx;
        windows = linux = osx = false;

        foreach (var item in rules) {
            if (item.Action == "allow") {
                if (item.System == null) {
                    windows = linux = osx = true;
                    continue;
                }

                foreach (var os in item.System) {
                    switch (os.Value) {
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
            } else if (item.Action == "disallow") {
                if (item.System == null) {
                    windows = linux = osx = false;
                    continue;
                }

                foreach (var os in item.System) {
                    switch (os.Value) {
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

        return EnvironmentUtil.GetPlatformName() switch {
            "windows" => windows,
            "linux" => linux,
            "osx" => osx,
            _ => false,
        };
    }

    private static LibraryEntry CreateLibraryEntryFromJsonNode(JsonNode libNode, string path) {
        var jsonNatives = libNode["natives"];
        var libJsonNode = libNode.Deserialize<LibraryJsonEntry>(JsonConverterUtil.DefaultJsonOptions);

        AppendNativeLibraryName(ref libJsonNode, jsonNatives);

        var relativePath = FormatLibraryNameToRelativePath(libJsonNode.Name);
        var absolutePath = Path.Combine(path, "libraries", relativePath);

        var librariesItemEntry = new LibraryEntry {
            Path = absolutePath,
            RelativePath = relativePath,
            IsNative = jsonNatives != null
        };

        GetLibraryChecksumAndUrl(ref librariesItemEntry, libNode, libJsonNode);

        return librariesItemEntry;
    }

    private static void AppendNativeLibraryName(ref LibraryJsonEntry libraryEntry, JsonNode jsonNatives) {
        if (jsonNatives != null && libraryEntry.Natives.TryGetValue(EnvironmentUtil.GetPlatformName(), out var value)) {
            libraryEntry.Name += $":{value.Replace("${arch}", EnvironmentUtil.Arch)}";
        }
    }

    private static void GetLibraryChecksumAndUrl(ref LibraryEntry libraryEntry, JsonNode jsonNode, LibraryJsonEntry libraryJsonNode) {
        if (libraryEntry.IsNative) {
            if (libraryJsonNode.Natives != null && libraryJsonNode.Natives
                .TryGetValue(EnvironmentUtil.GetPlatformName(), out string value)) {
                var nativeName = value.Replace("${arch}", EnvironmentUtil.Arch);

                libraryEntry.Checksum = libraryJsonNode.Downloads.Classifiers[nativeName].Sha1;
                libraryEntry.Size = libraryJsonNode.Downloads.Classifiers[nativeName].Size;
                libraryEntry.Url = libraryJsonNode.Downloads.Classifiers[nativeName].Url;
            }

            if (jsonNode.GetString("name").Contains("natives")) {
                libraryEntry.Checksum = libraryJsonNode.Downloads.Artifact.Sha1;
                libraryEntry.Size = libraryJsonNode.Downloads.Artifact.Size;
                libraryEntry.Url = libraryJsonNode.Downloads.Artifact.Url;
            }
            return;
        }

        if (libraryJsonNode.Downloads?.Artifact != null) {
            libraryEntry.Checksum = libraryJsonNode.Downloads.Artifact.Sha1;
            libraryEntry.Size = libraryJsonNode.Downloads.Artifact.Size;
            libraryEntry.Url = libraryJsonNode.Downloads.Artifact.Url;

            return;
        }

        if (libraryEntry.Url == null && jsonNode["url"] != null) {
            libraryEntry.Url = (jsonNode.GetString("url") + libraryEntry.RelativePath)
                .Replace('\\', '/');
        }

        if (libraryEntry.Checksum == null && jsonNode["checksums"] != null) {
            libraryEntry.Checksum = jsonNode["checksums"]!
                .AsArray()[0]!
                .GetString();
        }
    }
}