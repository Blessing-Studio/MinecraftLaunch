using MinecraftLaunch.Classes.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftLaunch.Classes.Interfaces {
    public interface IDownloadEntry {
        int Size { get; set; }

        string Url { get; set; }

        string Path { get; set; }

        string Checksum { get; set; }

        DownloadEntryType Type { get; }
    }
}
