using MinecraftLaunch.Classes.Models.Game;

namespace MinecraftLaunch.Extensions;

/// <summary>
/// Provides extension methods for game entries.
/// </summary>
public static class GameEntryExtension {
    #region String

    /// <summary>
    /// Gets the path of the launcher profile.
    /// </summary>
    /// <param name="root">The root directory.</param>
    /// <returns>The path of the launcher profile.</returns>
    public static string ToLauncherProfilePath(this string root) =>
        Path.Combine(root, "launcher_profiles.json");

    /// <summary>
    /// Gets the path of the launcher account.
    /// </summary>
    /// <param name="root">The root directory.</param>
    /// <returns>The path of the launcher account.</returns>
    public static string ToLauncherAccountPath(this string root) =>
        Path.Combine(root, "launcher_accounts.json");

    #endregion

    #region GameEntry

    /// <summary>
    /// Gets the path ofthe libraries directory.
    /// </summary>
    /// <param name="entry">The game entry.</param>
    /// <returns>The path of the libraries directory.</returns>
    public static string ToLibrariesPath(this GameEntry entry) =>
Path.Combine(entry.GameFolderPath, "libraries");

    /// <summary>
    /// Gets the path of the version JSON file.
    /// </summary>
    /// <param name="entry">The game entry.</param>
    /// <returns>The path of the version JSON file.</returns>
    public static string ToVersionJsonPath(this GameEntry entry) =>
        Path.Combine(entry.GameFolderPath, "versions", entry.Id, $"{entry.Id}.json");

    /// <summary>
    /// Gets the path of the version JRE file.
    /// </summary>
    /// <param name="entry">The game entry.</param>
    /// <returns>The path of the version JSON file.</returns>
    public static string ToVersionJarPath(this GameEntry entry) =>
        Path.Combine(entry.GameFolderPath, "versions", entry.Id, $"{entry.Id}.jar");

    /// <summary>
    /// Gets the path of the version directory.
    /// </summary>
    /// <param name="entry">The game entry.</param>
    /// <param name="Isolate">A value indicating whether to isolate the version directory.</param>
    /// <returns>The path of the version directory.</returns>
    public static string ToVersionDirectoryPath(this GameEntry entry, bool Isolate = true) => Isolate
        ? Path.Combine(entry.GameFolderPath, "versions", entry.Id)
        : entry.GameFolderPath;

    /// <summary>
    /// Gets the path of the mod directory.
    /// </summary>
    /// <param name="entry">The game entry.</param>
    /// <param name="Isolate">A value indicating whether to isolate the mod directory.</param>
    /// <returns>The path of the mod directory.</returns>
    public static string ToModDirectorypath(this GameEntry entry, bool Isolate = true) =>
        Path.Combine(entry.ToVersionDirectoryPath(Isolate), "mods");

    #endregion
}