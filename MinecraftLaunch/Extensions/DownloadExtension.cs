using MinecraftLaunch.Classes.Enums;
using MinecraftLaunch.Classes.Interfaces;
using MinecraftLaunch.Classes.Models.Event;
using MinecraftLaunch.Components.Checker;
using MinecraftLaunch.Classes.Models.Download;
using MinecraftLaunch.Components.Downloader;

namespace MinecraftLaunch.Extensions {
    /// <summary>
    /// 下载扩展类
    /// </summary>
    public static class DownloadExtension {
        public static FileDownloader DefaultDownloader { get; set; }

        public static IDownloadEntry OfMirrorSource(this IDownloadEntry entry,
            MirrorDownloadSource source) {
            if (MirrorDownloadManager.IsUseMirrorDownloadSource  && source is not null) {
                var urls = entry.Type is DownloadEntryType.Asset
                    ? source.AssetsUrls
                    : source.LibrariesUrls;
                
                entry.Url = entry.Url.Replace(urls);
            }

            return entry;
        }

        public static ValueTask<bool> DownloadResourceEntryAsync(this 
            IDownloadEntry downloadEntry,
            MirrorDownloadSource source = default!) {
            DefaultDownloader = new(downloadEntry
                .OfMirrorSource(source)
                .ToDownloadRequest());

            return DefaultDownloader.StartAsync();
        }

        public static ValueTask<bool> DownloadResourceEntrysAsync(this
            IEnumerable<IDownloadEntry> entries,
            MirrorDownloadSource source = default!,
            Action<DownloadProgressChangedEventArgs> action = default!) {
            DefaultDownloader = new(entries
                .Select(x => x.OfMirrorSource(source))
                .Select(x => x.ToDownloadRequest()));

            DefaultDownloader.ProgressChanged += (sender, args) => {
                action(args);
            };

            return DefaultDownloader.StartAsync();
        }

        public static double ToPercentage(this DownloadProgressChangedEventArgs args) {
            if (args.TotalCount > 1) {
                return (double)args.CompletedCount / args.TotalCount * 100;
            }

            return (double)args.DownloadedBytes / args.TotalBytes * 100;
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
}
