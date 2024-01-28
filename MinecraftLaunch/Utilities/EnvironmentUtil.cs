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

    public readonly static Dictionary<Func<bool>, Platform> PlatformDirectory = new() {
        { () => IsMac, Platform.osx },
        { () => IsLinux, Platform.linux },
        { () => IsWindow, Platform.windows }
    };

    public static Platform GetPlatformName() {
        foreach (var item in PlatformDirectory) {
            if (item.Key.Invoke()) {
                return item.Value;
            }
        }

        return Platform.unknown;
    }
}