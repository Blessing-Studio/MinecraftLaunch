using MinecraftLaunch.Classes.Enums;
using System.IO.Compression;

namespace MinecraftLaunch.Utilities;

public static class ZipUtil {
    public static void ExtractNatives(string targetFolder, IEnumerable<string> files) {
        if (!Directory.Exists(targetFolder)) {
            Directory.CreateDirectory(targetFolder);
        }

        DirectoryUtil.DeleteAllFiles(targetFolder);

        var extension = EnvironmentUtil.GetPlatformName() switch {
            Platform.windows => ".dll",
            Platform.linux => ".so",
            Platform.osx => ".dylib",
            _ => "."
        };

        foreach (var file in files) {
            using ZipArchive zip = ZipFile.OpenRead(file);

            foreach (ZipArchiveEntry entry in zip.Entries) {
                if (Path.GetExtension(entry.Name).Contains(extension)) {
                    entry.ExtractToFile(Path.Combine(targetFolder, entry.Name), true);
                }
            }
        }
    }
}