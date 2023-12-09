using MinecraftLaunch.Classes.Enums;
using System.Collections.Immutable;

namespace MinecraftLaunch.Classes.Models.Game {
    public record ModEntry {
        public string Path { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public string Version { get; set; }

        public bool IsEnabled { get; set; }

        public bool IsError { get; set; }

        public ImmutableArray<string> Authors { get; set; }

        public LoaderType[] SupportedModLoaders { get; set; }
    }

    public record ForgeModEntry {
        public ForgeModEntry[] modList { get; set; }

        public string name { get; set; }

        public string description { get; set; }

        public string version { get; set; }

        public string mcversion { get; set; }

        public string[] authors { get; set; }

        public string[] authorList { get; set; }

        public string url { get; set; }
    }
}
