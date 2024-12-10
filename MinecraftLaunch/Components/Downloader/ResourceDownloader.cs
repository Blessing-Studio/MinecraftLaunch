using MinecraftLaunch.Classes.Enums;
using MinecraftLaunch.Classes.Interfaces;
using MinecraftLaunch.Classes.Models.Download;
using MinecraftLaunch.Classes.Models.Event;
using MinecraftLaunch.Components.Checker;
using MinecraftLaunch.Extensions;

namespace MinecraftLaunch.Components.Downloader;

/// <summary>
/// 游戏资源下载器
/// </summary>
public sealed class ResourceDownloader {
    private readonly FileDownloader _downloader;
    private readonly ResourceChecker _resourceChecker;
    private readonly DownloaderConfiguration _downloaderConfiguration;

    public event EventHandler<DownloadProgressChangedEventArgs> ProgressChanged;

    public ResourceDownloader(DownloaderConfiguration configuration = default) {
        _downloaderConfiguration = configuration ?? DownloaderConfiguration.Default;
        _downloader = new(_downloaderConfiguration);
    }

    public ResourceDownloader(ResourceChecker resourceChecker, DownloaderConfiguration configuration = default) {
        _downloaderConfiguration = configuration ?? DownloaderConfiguration.Default;
        _resourceChecker = resourceChecker ??
            throw new NullReferenceException("The 'ResourceChecker' object refers to an instance that has not been set as an object");

        _downloader = new(_downloaderConfiguration);
    }

    public async Task<GroupDownloadResult> CheckAndDownloadAsync(CancellationToken cancellation = default) {
        var result = await _resourceChecker.CheckAsync();
        if (result) {
            return new GroupDownloadResult {
                Failed = default,
                Type = DownloadResultType.Successful
            };
        }

        return await DownloadAsync(_resourceChecker.MissingResources, cancellation);
    }

    public async Task<GroupDownloadResult> DownloadAsync(IEnumerable<IDownloadEntry> downloadEntries, CancellationToken cancellation = default) {
        double speed = 0;
        int currentCount = 0;
        int totalCount = downloadEntries.Count();

        var entries = downloadEntries.Select(x => {
            if (string.IsNullOrEmpty(x.Url)) {
                return x.ToDownloadRequest();
            }

            return x.OfMirrorSource().ToDownloadRequest();
        });

        var req = new GroupDownloadRequest(entries) {
            DownloadSpeedChanged = s => speed = s,

            SingleRequestCompleted = (dreq, dres) => {
                Interlocked.Increment(ref currentCount);
                ProgressChanged?.Invoke(this, new() {
                    Speed = speed,
                    TotalCount = totalCount,
                    CompletedCount = currentCount,
                });
            }
        };

        return await _downloader.DownloadFilesAsync(req, cancellation);
    }
}