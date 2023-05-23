using System;
using System.Runtime.InteropServices;

namespace MinecraftLaunch.Modules.Toolkits;

public class EnvironmentToolkit
{
	public static string Arch
	{
		get
		{
			if (!Environment.Is64BitOperatingSystem)
			{
				return "32";
			}
			return "64";
		}
	}

	public static string GetPlatformName()
	{
		if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
		{
			return "osx";
		}
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
		{
			return "linux";
		}
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			return "windows";
		}
		return "unknown";
	}
}
