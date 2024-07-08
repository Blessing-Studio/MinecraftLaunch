using MinecraftLaunch.Classes.Models.Game;

namespace MinecraftLaunch.Extensions; 

/// <summary>
/// Provides extension methods for game entries.
/// </summary>
public static class GameEntryExtension {
    /// <summary>
    /// Gets the path of the launcher profile.
    /// </summary>
    /// <param name="root">The root directory.</param>
    /// <returns>The path of the launcher profile.</returns>
    public static string OfLauncherProfilePath(this string root) =>
        Path.Combine(root, "launcher_profiles.json");

    /// <summary>
    /// Gets the path of the launcher account.
    /// </summary>
    /// <param name="root">The root directory.</param>
    /// <returns>The path of the launcher account.</returns>
    public static string OfLauncherAccountPath(this string root) =>
        Path.Combine(root, "launcher_accounts.json");

    /// <summary>
    /// Gets the path of the version JSON file.
    /// </summary>
    /// <param name="entry">The game entry.</param>
    /// <returns>The path of the version JSON file.</returns>
    public static string OfVersionJsonPath(this GameEntry entry) =>
        Path.Combine(entry.GameFolderPath, "versions", entry.Id, $"{entry.Id}.json");

    /// <summary>
    /// Gets the path of the version directory.
    /// </summary>
    /// <param name="entry">The game entry.</param>
    /// <param name="Isolate">A value indicating whether to isolate the version directory.</param>
    /// <returns>The path of the version directory.</returns>
    public static string OfVersionDirectoryPath(this GameEntry entry, bool Isolate = true) => Isolate
        ? Path.Combine(entry.GameFolderPath, "versions", entry.Id)
        : entry.GameFolderPath;

    /// <summary>
    /// Gets the path of the mod directory.
    /// </summary>
    /// <param name="entry">The game entry.</param>
    /// <param name="Isolate">A value indicating whether to isolate the mod directory.</param>
    /// <returns>The path of the mod directory.</returns>
    public static string OfModDirectorypath(this GameEntry entry, bool Isolate = true) => Path
        .Combine(entry.OfVersionDirectoryPath(Isolate), "mods");
}