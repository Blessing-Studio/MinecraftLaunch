using System;
using System.Runtime.InteropServices;

namespace MinecraftLaunch.Modules.Utilities;

public class EnvironmentUtil {
    public static string Arch
    {
        get
        {
            if (!Environment.Is64BitOperatingSystem) {
                return "32";
            }
            return "64";
        }
    }

    public readonly static bool IsMac = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

    public readonly static bool IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    public readonly static bool IsWindow = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    public static string GetPlatformName() {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
            return "osx";
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
            return "linux";
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            return "windows";
        }
        return "unknown";
    }
}
