using MinecraftLaunch.Classes.Enums;
using MinecraftLaunch.Classes.Interfaces;

namespace MinecraftLaunch.Classes.Models.Game {
    public record JarEntry : IDownloadEntry {
        public int Size { get; set; }

        public string Url { get; set; }

        public string Checksum { get; set; }

        public string Path { get; set; }

        public string McVersion { get; set; }

        public DownloadEntryType Type => DownloadEntryType.Jar;
    }
}
