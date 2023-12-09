using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftLaunch.Extensions {
    public static class ZipArchiveExtension {
        public static string ReadAsString(this ZipArchiveEntry archiveEntry) {
            using var stream = archiveEntry.Open();
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
