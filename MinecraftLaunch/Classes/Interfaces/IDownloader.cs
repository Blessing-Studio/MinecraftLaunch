using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftLaunch.Classes.Interfaces {
    public interface IDownloader {
        ValueTask<bool> DownloadAsync();
    }
}
