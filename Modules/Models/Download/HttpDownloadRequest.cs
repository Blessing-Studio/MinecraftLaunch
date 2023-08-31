using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftLaunch.Modules.Models.Download {
    public class HttpDownloadRequest {
        public DirectoryInfo Directory { get; set; }

        public string Url { get; set; }

        public int? Size { get; set; }

        public string Sha1 { get; set; }

        public string FileName { get; set; }
    }
}
