using MinecraftLaunch.Classes.Enums;
using MinecraftLaunch.Classes.Interfaces;
using MinecraftLaunch.Classes.Models.Event;
using MinecraftLaunch.Classes.Models.Download;
using MinecraftLaunch.Components.Downloader;
using MinecraftLaunch.Classes.Models.Game;
using MinecraftLaunch.Utilities;

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

    public static ValueTask<bool> DownloadAsync(this DownloadRequest request,
        Action<DownloadProgressChangedEventArgs> action = default!) {
        //DefaultDownloader.Setup(Enumerable.Repeat(request, 1));

        //DefaultDownloader.ProgressChanged += (sender, args) => {
        //    action(args);
        //};

        //return DefaultDownloader.DownloadAsync();
        throw new NotImplementedException();
    }

    public static ValueTask<bool> DownloadResourceEntryAsync(this 
        IDownloadEntry downloadEntry,
        MirrorDownloadSource source = default!) {
        return DownloadUitl.DownloadAsync(downloadEntry, DownloadUitl.DefaultDownloadRequest,default,x=>{});

        //DefaultDownloader.Setup(Enumerable.Repeat(downloadEntry
        //    .OfMirrorSource(source)
        //    .ToDownloadRequest(), 1));

        //Console.WriteLine(downloadEntry.Path);
        //return DefaultDownloader.DownloadAsync();
        //throw new NotImplementedException();
    }

    public static ValueTask<bool> DownloadResourceEntrysAsync(this
        IEnumerable<IDownloadEntry> entries,
        MirrorDownloadSource source = default!,
        Action<DownloadProgressChangedEventArgs> action = default!,
        DownloadRequest downloadRequest = default!) {
        downloadRequest ??= DownloadUitl.DefaultDownloadRequest;

        if (MirrorDownloadManager.IsUseMirrorDownloadSource && source is not null) {
            entries.Select(x => {
                if (x.Type is DownloadEntryType.Jar) {
                    x.Url = $"{source.Host}/version/{(x as JarEntry).McVersion}/client";
                } else {
                    x.OfMirrorSource(source);
                }

                return x;
            });
        }

        ResourceDownloader downloader = new(entries, downloadRequest, source);
        downloader.ProgressChanged += (sender, args) => {
            action(args);
        };

        return downloader.DownloadAsync();
    }

    public static double ToPercentage(this DownloadProgressChangedEventArgs args) {
        return (double)args.CompletedCount / (double)args.TotalCount;
    }

    public static double ToPercentage(this double progress, double mini, double max) {
        return mini + (max - mini) * progress;
    }
}