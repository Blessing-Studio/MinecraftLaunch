using MinecraftLaunch.Classes.Models.Game;

namespace MinecraftLaunch.Extensions {
    public static class GameEntryExtension {
        public static string OfVersionJsonPath(this GameEntry entry) =>
            Path.Combine(entry.GameFolderPath, "versions", entry.Id, $"{entry.Id}.json");

        public static string OfVersionDirectoryPath(this GameEntry entry, bool Isolate = true) => Isolate
            ? Path.Combine(entry.GameFolderPath, "versions", entry.Id)
            : entry.GameFolderPath;

        public static string OfModDirectorypath(this GameEntry entry, bool Isolate = true) => Path
            .Combine(entry.OfVersionDirectoryPath(Isolate), "mods");
    }
}
