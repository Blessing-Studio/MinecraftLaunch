using System.Text.Json;
using System.Text.Json.Nodes;
using MinecraftLaunch.Utilities;
using MinecraftLaunch.Extensions;
using MinecraftLaunch.Classes.Interfaces;
using MinecraftLaunch.Classes.Models.Game;
using MinecraftLaunch.Classes.Enums;

namespace MinecraftLaunch.Components.Resolver;

/// <summary>
/// Minecraft 运行库解析器
/// </summary>
class LibrariesResolver(GameEntry gameEntry) : IResolver<LibraryEntry, JsonNode> {
    public GameEntry GameEntry => gameEntry;

    public LibraryEntry Resolve(JsonNode libNode) {
        var jsonRules = libNode["rules"];
        var jsonNatives = libNode["natives"];

        if (jsonRules != null && !GetLibraryEnable(jsonRules.Deserialize<IEnumerable<RuleModel>>(JsonConverterUtil
            .DefaultJsonConverterOptions)!)) {
            return null!;
        }

        var libraryEntry = libNode.Deserialize<LibraryJsonEntry>();
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

    public IEnumerable<LibraryEntry> GetLibraries() {
        var jsonPath = Path.Combine(GameEntry.OfVersionJsonPath());
        var libsNode = JsonNode.Parse(File.ReadAllText(jsonPath))!["libraries"]!.AsArray();
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

    public IEnumerable<LibraryEntry> GetLibrariesFromJsonArray(JsonArray jsonArray) {
        foreach (var libNode in jsonArray) {
            var libraryEntry = CreateLibraryEntryFromJsonNode(libNode);
            if (libraryEntry != null) {
                yield return libraryEntry;
            }
        }
    }

    private LibraryEntry GetJarEntry() {
        var jsonClient = JsonNode.Parse(File.ReadAllText(GameEntry
            .OfVersionJsonPath()))?["downloads"]?["client"];

        if (jsonClient != null)
            return new LibraryEntry {
                Path = GameEntry.JarPath,
                Url = jsonClient.GetString("url"),
                Size = jsonClient.GetInt32("size"),
                Checksum = jsonClient.GetString("sha1")
            };

        return null;
    }

    private IEnumerable<string> FormatLibraryName(string Name) {
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
                        case Platform.windows:
                            windows = true;
                            break;
                        case Platform.linux:
                            linux = true;
                            break;
                        case Platform.osx:
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
                        case Platform.windows:
                            windows = false;
                            break;
                        case Platform.linux:
                            linux = false;
                            break;
                        case Platform.osx:
                            osx = false;
                            break;
                    }
                }
            }
        }

        return EnvironmentUtil.GetPlatformName() switch {
            Platform.windows => windows,
            Platform.linux=> linux,
            Platform.osx => osx,
            _ => false,
        };
    }

    private string FormatLibraryNameToRelativePath(string name) {
        string path = string.Empty;
        foreach (var subPath in FormatLibraryName(name)) {
            path = Path.Combine(path, subPath);
        }

        return path;
    }

    private LibraryEntry CreateLibraryEntryFromJsonNode(JsonNode libNode) {
        var jsonNatives = libNode["natives"];
        var libJsonNode = libNode.Deserialize<LibraryJsonEntry>();

        AppendNativeLibraryName(ref libJsonNode, jsonNatives);

        var relativePath = FormatLibraryNameToRelativePath(libJsonNode.Name);
        var absolutePath = Path.Combine(GameEntry.GameFolderPath, "libraries", relativePath);

        var librariesItemEntry = new LibraryEntry {
            Path = absolutePath,
            RelativePath = relativePath,
            IsNative = jsonNatives != null
        };

        GetLibraryChecksumAndUrl(ref librariesItemEntry, libNode, libJsonNode);

        return librariesItemEntry;
    }

    private void AppendNativeLibraryName(ref LibraryJsonEntry libraryEntry, JsonNode jsonNatives) {
        if (jsonNatives != null && libraryEntry.Natives.TryGetValue(EnvironmentUtil.GetPlatformName(), out var value)) {
            libraryEntry.Name += $":{value.Replace("${arch}", EnvironmentUtil.Arch)}";
        }
    }

    private void GetLibraryChecksumAndUrl(ref LibraryEntry libraryEntry, JsonNode jsonNode, LibraryJsonEntry libraryJsonNode) {
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