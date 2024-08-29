using System.IO.Compression;

namespace MinecraftLaunch.Extensions;

/// <summary>
/// Provides extension methods for ZipArchiveEntry.
/// </summary>
public static class ZipArchiveExtension {

    /// <summary>
    /// Reads the contents of the ZipArchiveEntry as a string.
    /// </summary>
    /// <param name="archiveEntry">The ZipArchiveEntry to read from.</param>
    /// <returns>A string containing the contents of the ZipArchiveEntry.</returns>
    public static string ReadAsString(this ZipArchiveEntry archiveEntry) {
        using var stream = archiveEntry.Open();
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Extracts the ZipArchiveEntry to the specified file.
    /// </summary>
    /// <param name="zipArchiveEntry">The ZipArchiveEntry to extract.</param>
    /// <param name="filename">The name of the file to extract to.</param>
    public static void ExtractTo(this ZipArchiveEntry zipArchiveEntry, string filename) {
        var file = new FileInfo(filename);

        if (!file.Directory.Exists)
            file.Directory.Create();

        zipArchiveEntry.ExtractToFile(filename, true);
    }
}