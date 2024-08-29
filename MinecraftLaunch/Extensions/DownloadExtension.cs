using MinecraftLaunch.Classes.Enums;
using MinecraftLaunch.Classes.Interfaces;
using MinecraftLaunch.Classes.Models.Download;
using MinecraftLaunch.Classes.Models.Event;
using MinecraftLaunch.Classes.Models.Game;
using MinecraftLaunch.Components.Downloader;
using MinecraftLaunch.Utilities;

namespace MinecraftLaunch.Extensions;

public static class DownloadExtension {

    /// <summary>
    /// Applies the mirror source to the download entry if the use of mirror download source is enabled.
    /// </summary>
    /// <param name="entry">The download entry to which the mirror source is to be applied.</param>
    /// <param name="source">The mirror download source to be applied.</param>
    /// <returns>The download entry with the applied mirror source.</returns>
    public static IDownloadEntry OfMirrorSource(this IDownloadEntry entry,
        MirrorDownloadSource source) {
        if (MirrorDownloadManager.IsUseMirrorDownloadSource && source is not null) {
            if (entry.Type is DownloadEntryType.Jar) {
                entry.Url = $"{source.Host}/version/{(entry as JarEntry).McVersion}/client";
            }

            var urls = entry.Type is DownloadEntryType.Asset
                ? source.AssetsUrls
                : source.LibrariesUrls;

            entry.Url = entry.Url.Replace(urls);
        }

        return entry;
    }

    /// <summary>
    /// Initiates an asynchronous download operation for the specified download request.
    /// </summary>
    /// <param name="request">The download request to be processed.</param>
    /// <param name="action">The action to be performed during the download operation.</param>
    /// <returns>A ValueTask that represents the asynchronous download operation. The task result contains a boolean value that indicates whether the download operation was successful.</returns>
    public static ValueTask<bool> DownloadAsync(
        this DownloadRequest request,
        Action<double> action = default!) {
        return DownloadUitl.DownloadAsync(request, default, action);
    }

    /// <summary>
    /// Initiates an asynchronous download operation for the specified download entry.
    /// </summary>
    /// <param name="downloadEntry">The download entry to be downloaded.</param>
    /// <param name="source">The mirror download source to be used.</param>
    /// <returns>A ValueTask that represents the asynchronous download operation. The task result contains a boolean value that indicates whether the download operation was successful.</returns>
    public static ValueTask<bool> DownloadResourceEntryAsync(this
        IDownloadEntry downloadEntry,
        MirrorDownloadSource source = default!) {
        return DownloadUitl.DownloadAsync(downloadEntry, DownloadUitl.DefaultDownloadRequest, default, x => { });
    }

    /// <summary>
    /// Initiates an asynchronous download operation for the specified collection of download entries.
    /// </summary>
    /// <param name="entries">The collection of download entries to be downloaded.</param>
    /// <param name="source">The mirror download source to be used.</param>
    /// <param name="action">The action to be performed during the download operation.</param>
    /// <param name="downloadRequest">The download request to be processed.</param>
    /// <returns>A ValueTask that represents the asynchronous download operation. The task result contains a boolean value that indicates whether the download operation was successful.</returns>
    public static ValueTask<bool> DownloadResourceEntrysAsync(this
        IEnumerable<IDownloadEntry> entries,
        MirrorDownloadSource source = default!,
        Action<DownloadProgressChangedEventArgs> action = default!,
        DownloadRequest downloadRequest = default!) {
        downloadRequest ??= DownloadUitl.DefaultDownloadRequest;

        if (MirrorDownloadManager.IsUseMirrorDownloadSource && source is not null) {
            entries = entries.Select(x => {
                if (x.Type is DownloadEntryType.Jar) {
                    x.Url = $"{source.Host}/version/{(x as JarEntry).McVersion}/client";
                } else {
                    x.OfMirrorSource(source);
                }

                return x;
            });
        }

        ResourceDownloader downloader = new(downloadRequest, entries, source);
        downloader.ProgressChanged += (sender, args) => {
            action(args);
        };

        return downloader.DownloadAsync();
    }

    /// <summary>
    /// Converts the download progress to a percentage.
    /// </summary>
    /// <param name="args">The download progress arguments.</param>
    /// <returns>The download progress as a percentage.</returns>
    public static double ToPercentage(this DownloadProgressChangedEventArgs args) {
        return (double)args.CompletedCount / (double)args.TotalCount;
    }

    /// <summary>
    /// Converts the specified progress value to a percentage within the specified range.
    /// </summary>
    /// <param name="progress">The progress value to be converted.</param>
    /// <param name="mini">The minimum value of the range.</param>
    /// <param name="max">The maximum value of the range.</param>
    /// <returns>The progress value as a percentage within the specified range.</returns>
    public static double ToPercentage(this double progress, double mini, double max) {
        return mini + (max - mini) * progress;
    }
}