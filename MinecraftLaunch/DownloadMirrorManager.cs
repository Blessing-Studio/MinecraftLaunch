using MinecraftLaunch.Base.Interfaces;

namespace MinecraftLaunch;

public static class DownloadMirrorManager {
    public static int MaxThread { get; set; } = 64;
    public static bool IsEnableMirror { get; set; }

    public static readonly IDownloadMirror BmclApi = new BmclApiSource();
}

public sealed class BmclApiSource : IDownloadMirror {
    private static readonly Dictionary<string, string> _replacementMap = new() {
        { "https://resources.download.minecraft.net", "https://bmclapi2.bangbang93.com/assets" },
        { "https://piston-meta.mojang.com", "https://bmclapi2.bangbang93.com" },
        { "https://launchermeta.mojang.com", "https://bmclapi2.bangbang93.com" },
        { "https://launcher.mojang.com" , "https://bmclapi2.bangbang93.com" },
        { "https://libraries.minecraft.net", "https://bmclapi2.bangbang93.com/maven" },
        { "https://maven.minecraftforge.net", "https://bmclapi2.bangbang93.com/maven" },
        { "https://files.minecraftforge.net/maven", "https://bmclapi2.bangbang93.com/maven" },
        { "https://maven.fabricmc.net", "https://bmclapi2.bangbang93.com/maven" },
        { "https://meta.fabricmc.net", "https://bmclapi2.bangbang93.com/fabric-meta" },
        { "https://maven.neoforged.net/releases/net/neoforged/forge", "https://bmclapi2.bangbang93.com/maven/net/neoforged/forge" }
    };

    public string TryFindUrl(string sourceUrl) {
        if (!DownloadMirrorManager.IsEnableMirror) {
            return sourceUrl;
        }

        foreach (var (src, mirror) in _replacementMap) {
            if (sourceUrl.StartsWith(src))
                return sourceUrl.Replace(src, mirror);
        }

        return sourceUrl;
    }
}