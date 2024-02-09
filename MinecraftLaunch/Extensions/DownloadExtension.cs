using MinecraftLaunch.Classes.Enums;
using MinecraftLaunch.Classes.Interfaces;
using MinecraftLaunch.Classes.Models.Event;
using MinecraftLaunch.Classes.Models.Download;
using MinecraftLaunch.Components.Downloader;
using MinecraftLaunch.Classes.Models.Game;

namespace MinecraftLaunch.Extensions;

/// <summary>
/// 下载扩展类
/// </summary>
public static class DownloadExtension {
    public static BatchDownloader DefaultDownloader { get; set; } = new();

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

    public static ValueTask<bool> DownloadAsync(this
            DownloadRequest request,
        Action<DownloadProgressChangedEventArgs> action = default!) {
        DefaultDownloader.Setup(Enumerable.Repeat(request, 1));

        DefaultDownloader.ProgressChanged += (sender, args) => {
            action(args);
        };

        return DefaultDownloader.DownloadAsync();
    }

    public static ValueTask<bool> DownloadResourceEntryAsync(this 
            IDownloadEntry downloadEntry,
        MirrorDownloadSource source = default!) {
        DefaultDownloader.Setup(Enumerable.Repeat(downloadEntry
            .OfMirrorSource(source)
            .ToDownloadRequest(), 1));

        Console.WriteLine(downloadEntry.Path);
        return DefaultDownloader.DownloadAsync();
    }

    public static ValueTask<bool> DownloadResourceEntrysAsync(this
            IEnumerable<IDownloadEntry> entries,
        MirrorDownloadSource source = default!,
        Action<DownloadProgressChangedEventArgs> action = default!) {
        DefaultDownloader.Setup(entries
            .Select(x => x.OfMirrorSource(source))
            .Select(x => x.ToDownloadRequest()));

        DefaultDownloader.ProgressChanged += (sender, args) => {
            action(args);
        };

        return DefaultDownloader.DownloadAsync();
    }

    public static double ToPercentage(this DownloadProgressChangedEventArgs args) {
        return (double)args.DownloadedBytes / (double)args.TotalBytes;
    }

    public static double ToPercentage(this double progress, double mini, double max) {
        return mini + (max - mini) * progress;
    }

    public static string ToSpeedText(this DownloadProgressChangedEventArgs args) {
        double speed = args.Speed;
        if (speed < 1024.0) {
            return speed.ToString("0") + " B/s";
        }

        if (speed < 1024.0 * 1024.0) {
            return (speed / 1024.0).ToString("0.0") + " KB/s";
        }

        if (speed < 1024.0 * 1024.0 * 1024.0) {
            return (speed / (1024.0 * 1024.0)).ToString("0.00") + " MB/s";
        }

        return "0";
    }
}