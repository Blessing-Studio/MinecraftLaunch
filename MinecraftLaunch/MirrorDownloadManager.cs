using MinecraftLaunch.Classes.Models.Download;

namespace MinecraftLaunch;

/// <summary>
/// 默认的下载镜像源管理类
/// </summary>
public static class MirrorDownloadManager {
    public static bool IsUseMirrorDownloadSource { get; set; } = false;

    public static readonly MirrorDownloadSource Bmcl = new() {
        Host = "https://bmclapi2.bangbang93.com",
        VersionManifestUrl = "https://bmclapi2.bangbang93.com/mc/game/version_manifest_v2.json",
        AssetsUrls = new Dictionary<string, string>() {
            { "https://resources.download.minecraft.net", "https://bmclapi2.bangbang93.com/assets" },
            { "https://piston-meta.mojang.com", "https://bmclapi2.bangbang93.com" },
            { "https://launchermeta.mojang.com", "https://bmclapi2.bangbang93.com" },
        },
        LibrariesUrls = new Dictionary<string, string>() {
            { "https://launcher.mojang.com" , "https://bmclapi2.bangbang93.com" },
            { "https://libraries.minecraft.net", "https://bmclapi2.bangbang93.com/maven" },
            { "https://piston-meta.mojang.com", "https://bmclapi2.bangbang93.com" },
            { "https://maven.minecraftforge.net", "https://bmclapi2.bangbang93.com/maven" },
            { "https://files.minecraftforge.net/maven", "https://bmclapi2.bangbang93.com/maven" },
            { "https://maven.fabricmc.net", "https://bmclapi2.bangbang93.com/maven" },
            { "https://meta.fabricmc.net", "https://bmclapi2.bangbang93.com/fabric-meta" },
            { "https://maven.neoforged.net/releases/net/neoforged/forge", "https://bmclapi2.bangbang93.com/maven/net/neoforged/forge" }
        }
    };
}