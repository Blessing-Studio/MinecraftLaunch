using System.Runtime.InteropServices;

namespace MinecraftLaunch.Utilities;

public static class EnvironmentUtil {

    public static string Arch
        => Environment.Is64BitOperatingSystem ? "64" : "32";

    public static bool IsMac
        => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

    public static bool IsLinux
        => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    public static bool IsWindow
        => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    public static string GetPlatformName() {
        if (IsMac) {
            return "osx";
        } else if (IsLinux) {
            return "linux";
        } else if (IsWindow) {
            return "windows";
        }

        return "114514";
    }
}