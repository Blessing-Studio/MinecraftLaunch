using MinecraftLaunch.Classes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftLaunch.Classes.Models.Download {
    /// <summary>
    /// 资源补全器返回信息
    /// </summary>
    public record ResourceDownloadResponse {
        public required IEnumerable<IDownloadEntry> FailedResources { get; set; }
    }
}
