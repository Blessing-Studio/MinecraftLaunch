using System.Collections.Generic;

namespace MinecraftLaunch.Modules.Models.Download;

public class APIManager
{
    public static DownloadAPI Current { get; set; } = new DownloadAPI
    {
        Host = "https://download.mcbbs.net",
        VersionManifest = "https://download.mcbbs.net/mc/game/version_manifest.json",
        Assets = "https://download.mcbbs.net/assets",
        Libraries = "https://download.mcbbs.net/maven"
    };

    public static readonly DownloadAPI Mojang = new DownloadAPI
	{
		Host = "https://launcher.mojang.com",
		VersionManifest = "http://launchermeta.mojang.com/mc/game/version_manifest.json",
		Assets = "http://resources.download.minecraft.net",
		Libraries = "https://libraries.minecraft.net"
	};

	public static readonly DownloadAPI Bmcl = new DownloadAPI
	{
		Host = "https://bmclapi2.bangbang93.com",
		VersionManifest = "https://bmclapi2.bangbang93.com/mc/game/version_manifest.json",
		Assets = "https://bmclapi2.bangbang93.com/assets",
		Libraries = "https://bmclapi2.bangbang93.com/maven"
	};

	public static readonly DownloadAPI Mcbbs = new DownloadAPI
	{
		Host = "https://download.mcbbs.net",
		VersionManifest = "https://download.mcbbs.net/mc/game/version_manifest.json",
		Assets = "https://download.mcbbs.net/assets",
		Libraries = "https://download.mcbbs.net/maven"
	};

	public static readonly Dictionary<string, string> ForgeLibraryUrlReplace = new Dictionary<string, string>
	{
		{
			"https://maven.minecraftforge.net",
			(Current.Host.Equals(Mojang.Host) ? "https://maven.minecraftforge.net" : Current.Libraries) ?? ""
		},
		{
			"https://files.minecraftforge.net/maven",
			(Current.Host.Equals(Mojang.Host) ? "https://maven.minecraftforge.net" : Current.Libraries) ?? ""
		}
	};

	public static readonly Dictionary<string, string> FabricLibraryUrlReplace = new Dictionary<string, string>
	{
		{
			"https://maven.fabricmc.net",
			(Current.Host.Equals(Mojang.Host) ? "https://maven.fabricmc.net" : Current.Libraries) ?? ""
		},
		{
			"https://meta.fabricmc.net",
			(Current.Host.Equals(Mojang.Host) ? "https://meta.fabricmc.net" : (Current.Host + "/fabric-meta")) ?? ""
		}
	};
}
