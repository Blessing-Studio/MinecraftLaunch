using MinecraftLaunch.Classes.Enums;
using System.Runtime.InteropServices;

namespace MinecraftLaunch.Utilities {
    public class EnvironmentUtil {
        public static string Arch
            => Environment.Is64BitOperatingSystem ? "64" : "32";

        public static bool IsMac
            => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        public static bool IsLinux
            => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        public static bool IsWindow
            => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public static Platform GetPlatformName() {
            if (IsMac) {
                return Platform.osx;
            } else if (IsLinux) {
                return Platform.linux;
            } else if (IsWindow) {
                return Platform.windows;
            }

            return Platform.unknown;
        }
    }
}
