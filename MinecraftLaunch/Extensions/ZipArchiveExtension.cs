using System.IO.Compression;

namespace MinecraftLaunch.Extensions;

public static class ZipArchiveExtension {
    public static string ReadAsString(this ZipArchiveEntry archiveEntry) {
        using var stream = archiveEntry.Open();
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    public static void ExtractTo(this ZipArchiveEntry zipArchiveEntry, string filename) {
        var file = new FileInfo(filename);

        if (!file.Directory.Exists)
            file.Directory.Create();

        zipArchiveEntry.ExtractToFile(filename, true);
    }
}