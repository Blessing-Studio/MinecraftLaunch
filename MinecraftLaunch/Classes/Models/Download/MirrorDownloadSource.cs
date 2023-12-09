using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftLaunch.Classes.Models.Download {
    /// <summary>
    /// 镜像下载源
    /// </summary>
    public record MirrorDownloadSource {
        public required string Host { get; set; }
        public required string VersionManifestUrl { get; set; }
        public required Dictionary<string, string> AssetsUrls { get; set; }
        public required Dictionary<string, string> LibrariesUrls { get; set; }

    }
}
