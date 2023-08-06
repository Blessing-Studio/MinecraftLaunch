using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using MinecraftLaunch.Modules.Models.Download;

namespace MinecraftLaunch.Modules.Utils;

public class ZipUtil {
    public static void GameNativesDecompress(DirectoryInfo directory, IEnumerable<LibraryResource> libraryResources) {
        if (!directory.Exists) {
            directory.Create();
        }
        directory.DeleteAllFiles();
        foreach (LibraryResource item in libraryResources.Where((LibraryResource x) => x.IsEnable && x.IsNatives)) {
            using ZipArchive zipArchive = ZipFile.OpenRead(item.ToFileInfo().FullName);
            foreach (ZipArchiveEntry entry in zipArchive.Entries) {
                try {
                    if (Path.GetExtension(entry.Name).Contains(".dll") || Path.GetExtension(entry.Name).Contains(".so") || Path.GetExtension(entry.Name).Contains(".dylib")) {
                        entry.ExtractToFile(Path.Combine(directory.FullName, entry.Name), overwrite: true);
                    }
                }
                catch (UnauthorizedAccessException) {
                }
            }
        }
    }
}
