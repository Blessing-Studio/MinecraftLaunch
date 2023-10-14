using MinecraftLaunch.Modules.Models.Download;
using MinecraftLaunch.Modules.Models.Launch;
using MinecraftLaunch.Modules.Utilities;

namespace MinecraftLaunch.Modules.Parser;

public class LibraryParser {
    public List<LibraryJsonEntity> Entities { get; set; }

    public DirectoryInfo Root { get; set; }

    public LibraryParser(List<LibraryJsonEntity> entities, DirectoryInfo root) {
        Entities = entities;
        Root = root;
    }

    public IEnumerable<LibraryResource> GetLibraries() {
        string platformName = EnvironmentUtil.GetPlatformName();
        foreach (LibraryJsonEntity libraryJsonEntity in Entities) {
            LibraryResource obj = new LibraryResource {
                CheckSum = (libraryJsonEntity.Downloads?.Artifact?.Sha1 ?? string.Empty),
                Size = (libraryJsonEntity.Downloads?.Artifact?.Size ?? 0),
                Url = (libraryJsonEntity.Downloads?.Artifact?.Url ?? string.Empty) + libraryJsonEntity.Url,
                Name = libraryJsonEntity.Name,
                Root = Root,
                IsEnable = true
            };

            if (libraryJsonEntity.Rules != null) {
                obj.IsEnable = GetAblility(libraryJsonEntity, platformName);
            }

            if (libraryJsonEntity.Natives != null) {
                obj.IsNatives = true;
                if (!libraryJsonEntity.Natives.ContainsKey(platformName)) {
                    obj.IsEnable = false;
                }

                if (obj.IsEnable) {
                    obj.Name += ":" + GetNativeName(libraryJsonEntity);
                    FileJsonEntity file = libraryJsonEntity.Downloads.Classifiers[libraryJsonEntity.Natives[platformName].Replace("${arch}", EnvironmentUtil.Arch)];
                    obj.CheckSum = file.Sha1;
                    obj.Size = file.Size;
                    obj.Url = file.Url;
                }
            }

            yield return obj;
        }
    }

    private string GetNativeName(LibraryJsonEntity libraryJsonEntity) {
        return libraryJsonEntity.Natives[EnvironmentUtil.GetPlatformName()].Replace("${arch}", EnvironmentUtil.Arch);
    }

    private bool GetAblility(LibraryJsonEntity libraryJsonEntity, string platform) {
        bool linux, osx, windows = osx = linux = false;
        foreach (RuleEntity item in libraryJsonEntity.Rules) {
            if (item.Action == "allow") {
                if (item.System == null) {
                    windows = linux = osx = true;
                    continue;
                }
                using Dictionary<string, string>.Enumerator enumerator2 = 
                    item.System.GetEnumerator();

                while (enumerator2.MoveNext()) {
                    switch (enumerator2.Current.Value) {
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
            } else {
                if (!(item.Action == "disallow")) {
                    continue;
                }

                if (item.System == null) {
                    windows = (linux = osx = false);
                    continue;
                }

                using Dictionary<string, string>.Enumerator enumerator2 =
                    item.System.GetEnumerator();

                while (enumerator2.MoveNext()) {
                    switch (enumerator2.Current.Value) {
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

        return platform switch {
            "windows" => windows,
            "linux" => linux,
            "osx" => osx,
            _ => false,
        };
    }
}
//private bool GetAblility(LibraryJsonEntity libraryJsonEntity, string platform) {
//    bool windows = false;
//    bool linux = false;
//    bool osx = false;

//    foreach (RuleEntity item in libraryJsonEntity.Rules) {
//        switch (item.Action) {
//            case "allow":
//                if (item.System == null) {
//                    windows = linux = osx = true;
//                } else {
//                    windows = item.System.ContainsKey("windows");
//                    linux = item.System.ContainsKey("linux");
//                    osx = item.System.ContainsKey("osx");
//                }
//                break;
//            case "disallow":
//                if (item.System == null) {
//                    windows = linux = osx = false;
//                } else {
//                    windows = !item.System.ContainsKey("windows");
//                    linux = !item.System.ContainsKey("linux");
//                    osx = !item.System.ContainsKey("osx");
//                }
//                break;
//        }
//    }

//    return platform switch {
//        "windows" => windows,
//        "linux" => linux,
//        "osx" => osx,
//        _ => false,
//    };
//}
