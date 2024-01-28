using MinecraftLaunch.Classes.Enums;
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

    public readonly static Dictionary<Func<bool>, string> PlatformDirectory = new() {
        { () => IsMac, "osx" },
        { () => IsLinux, "linux" },
        { () => IsWindow, "windows" }
    };

    public static string GetPlatformName() {
        foreach (var item in PlatformDirectory) {
            if (item.Key.Invoke()) {
                return item.Value;
            }
        }

        return "";
    }
}