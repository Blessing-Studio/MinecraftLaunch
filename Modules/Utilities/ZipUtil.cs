using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using MinecraftLaunch.Modules.Models.Download;

namespace MinecraftLaunch.Modules.Utilities;

public class ZipUtil {
    public static void DecompressGameNatives(DirectoryInfo targetDirectory, IEnumerable<LibraryResource> libraryResources) {
        if (!targetDirectory.Exists) {
            targetDirectory.Create();
        }

        targetDirectory.DeleteAllFiles();
        foreach (LibraryResource resource in libraryResources.Where(resource => resource.IsEnable && resource.IsNatives)) {
            using ZipArchive zipArchive = ZipFile.OpenRead(resource.ToFileInfo().FullName);
            foreach (ZipArchiveEntry entry in zipArchive.Entries) {
                try {
                    string fileExtension = Path.GetExtension(entry.Name);
                    if (fileExtension.Contains(".dll") || fileExtension.Contains(".so") || fileExtension.Contains(".dylib")) {
                        entry.ExtractToFile(Path.Combine(targetDirectory.FullName, entry.Name), overwrite: true);
                    }
                }
                catch (UnauthorizedAccessException) { }
            }
        }
    }
}
