using Flurl.Http;
using MinecraftLaunch.Classes.Enums;
using MinecraftLaunch.Classes.Interfaces;
using MinecraftLaunch.Classes.Models.Download;
using System.Buffers;
using System.Collections.Frozen;
using Timer = System.Timers.Timer;

namespace MinecraftLaunch.Components.Downloader;

public sealed class FileDownloader : IDownloader {
    private record class DownloaderConfig(
        long ChunkSize,
        int WorkersPerDownloadTask,
        int ConcurrentDownloadTasks,
        MirrorDownloadSource? DownloadSource);

    private class DownloadStates {
        public required string Url { get; init; }
        public required string LocalPath { get; init; }
        public long? TotalBytes { get; set; }

        // Used by chunk organization in multipart download
        public required long ChunkSize { get; init; }

        public long _chunkScheduled = 0;
        public long TotalChunks { get; set; } = 0;
        private readonly object _chunkOrganizerLock = new();

        public (long start, long end)? NextChunk() {
            long totalBytes = (long)TotalBytes!;
            long start, end;
            lock (_chunkOrganizerLock) {
                if (_chunkScheduled == TotalChunks)
                    return null;
                start = _chunkScheduled * ChunkSize;
                _chunkScheduled++;
            }
            // Handle the last chunk
            end = Math.Min(start + ChunkSize, totalBytes) - 1;
            return (start, end);
        }
    }


    public long ChunkSize => _config.ChunkSize;
    public int WorkersPerDownloadTask => _config.WorkersPerDownloadTask;
    public int ConcurrentDownloadTasks => _config.ConcurrentDownloadTasks;
    public MirrorDownloadSource? DownloadSource => _config.DownloadSource;

    private const int DOWNLOAD_BUFFER_SIZE = 4096;

    private readonly DownloaderConfig _config;
    private readonly SemaphoreSlim _globalDownloadTasksSemaphore;

    public FileDownloader(
        DownloaderConfiguration configuration) {
        _config = new DownloaderConfig(1048576, 8, configuration.MaxThread, configuration.DownloadSource);
        _globalDownloadTasksSemaphore = new SemaphoreSlim(8, configuration.MaxThread);
    }

    public static string GetSpeedText(double speed) {
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

    public async Task<DownloadResult> DownloadFileAsync(DownloadRequest request, CancellationToken cancellationToken = default) {
        try {
            cancellationToken.ThrowIfCancellationRequested();
            await _globalDownloadTasksSemaphore.WaitAsync(cancellationToken);
        } catch (OperationCanceledException) {
            return new DownloadResult(DownloadResultType.Cancelled);
        }

        try {
            await DownloadFileDriverAsync(request, cancellationToken);
            return new DownloadResult(DownloadResultType.Successful);
        } catch (TaskCanceledException) {
            return new DownloadResult(DownloadResultType.Cancelled);
        } catch (Exception e) {
            return new DownloadResult(DownloadResultType.Failed) {
                Exception = e
            };
        } finally {
            _globalDownloadTasksSemaphore.Release();
        }
    }

    public async Task<GroupDownloadResult> DownloadFilesAsync(GroupDownloadRequest request, CancellationToken cancellationToken = default) {
        Timer timer = new(TimeSpan.FromSeconds(1));
        List<Task> downloadTasks = [];
        Dictionary<DownloadRequest, DownloadResult> failed = [];
        long bytesReceived = 0;
        long previousBytesReceived = 0;

        timer.Elapsed += (_, _) => {
            TimeSpan elapsedTime = DateTime.Now - request.StartTime;
            long diffBytes = bytesReceived - previousBytesReceived;
            previousBytesReceived = bytesReceived;

            double speed = diffBytes / 1;
            request.DownloadSpeedChanged?.Invoke(speed);
        };

        foreach (var req in request.Files) {
            var r = req;
            string url = req.Url;
            string localPath = req.FileInfo.FullName;

            r.BytesDownloaded = b => {
                bytesReceived += b;
            };

            if (_config.DownloadSource is not null)
                url = MirrorDownloadManager.GetMirrorUrl(url, _config.DownloadSource);

            downloadTasks.Add(DownloadFileInGroupAsync(r, request, failed, cancellationToken));
        }

        timer.Start();
        await Task.WhenAll(downloadTasks);
        timer.Stop();

        DownloadResultType type = DownloadResultType.Successful;
        if (cancellationToken.IsCancellationRequested)
            type = DownloadResultType.Cancelled;
        else if (failed.Count > 0)
            type = DownloadResultType.Failed;

        return new GroupDownloadResult {
            Failed = failed.ToFrozenDictionary(),
            Type = type
        };
    }

    private async Task DownloadFileDriverAsync(DownloadRequest request, CancellationToken cancellationToken = default) {
        string url = request.Url;
        string localPath = request.FileInfo.FullName;

        if (_config.DownloadSource is not null) {
            url = MirrorDownloadManager.GetMirrorUrl(url, _config.DownloadSource);
        }

        (var flurlResponse, url) = await PrepareForDownloadAsync(url, cancellationToken);
        var httpResponse = flurlResponse.ResponseMessage;

        DownloadStates states = new() {
            Url = url,
            LocalPath = localPath,
            ChunkSize = _config.ChunkSize
        };

        bool useMultiPart = false;
        if (httpResponse.Content.Headers.ContentLength is long contentLength) {
            //request.Size = contentLength;
            states.TotalBytes = contentLength;

            var rangeResponse = await url.WithHeader("Range", "bytes=0-0")
                .GetAsync(cancellationToken: cancellationToken);

            useMultiPart = rangeResponse.StatusCode is 206;
        }

        // Status changed
        //request.FileSizeReceived?.Invoke(states.TotalBytes);

        string? destinationDir = Path.GetDirectoryName(localPath);
        if (destinationDir is not null)
            Directory.CreateDirectory(destinationDir);

        if (useMultiPart) {
            await DownloadMultiPartAsync(states, request, cancellationToken);
        } else {
            await DownloadSinglePartAsync(states, request, cancellationToken);
        }

    }

    private async Task<(IFlurlResponse Response, string RedirectedUrl)> PrepareForDownloadAsync(string url, CancellationToken cancellationToken = default) {
        // Get header
        var response = await url
            .SendAsync(HttpMethod.Head, completionOption: HttpCompletionOption.ResponseHeadersRead, cancellationToken: cancellationToken);

        if (response.StatusCode is 302) {
            var redirectUrl = response.ResponseMessage.Headers.Location?.AbsoluteUri;
            if (redirectUrl is not null)
                return await PrepareForDownloadAsync(redirectUrl, cancellationToken);
        }

        response.ResponseMessage.EnsureSuccessStatusCode();
        return (response, url);
    }

    private async Task WriteStreamToFile(Stream contentStream, FileStream fileStream, Memory<byte> buffer, DownloadRequest request, CancellationToken cancellationToken = default) {
        int bytesRead = 0;
        while ((bytesRead = await contentStream.ReadAsync(buffer, cancellationToken)) > 0) {
            await fileStream.WriteAsync(buffer[0..bytesRead], cancellationToken);
            request.BytesDownloaded?.Invoke(bytesRead);
        }
    }

    private async Task DownloadSinglePartAsync(DownloadStates states, DownloadRequest request, CancellationToken cancellationToken = default) {
        using var response = await states.Url.GetAsync(cancellationToken: cancellationToken);
        response.ResponseMessage.EnsureSuccessStatusCode();

        using var contentStream = await response.ResponseMessage.Content.ReadAsStreamAsync(cancellationToken);
        using var fileStream = new FileStream(states.LocalPath, FileMode.Create, FileAccess.Write);
        if (states.TotalBytes is long size)
            fileStream.SetLength(size);

        byte[] downloadBufferArr = ArrayPool<byte>.Shared.Rent(DOWNLOAD_BUFFER_SIZE);
        Memory<byte> downloadBuffer = downloadBufferArr.AsMemory(0, DOWNLOAD_BUFFER_SIZE);
        await WriteStreamToFile(contentStream, fileStream, downloadBuffer, request, cancellationToken);
        ArrayPool<byte>.Shared.Return(downloadBufferArr);
    }

    private async Task DownloadMultiPartAsync(DownloadStates states, DownloadRequest request, CancellationToken cancellationToken = default) {
        long fileSize = (long)states.TotalBytes!; // Not null in multipart download

        long totalChunks = Math.DivRem(fileSize, ChunkSize, out long remainder);
        if (remainder > 0)
            totalChunks++;
        states.TotalChunks = totalChunks;

        // Pre-allocate the file with the desired size
        using var fileStream = new FileStream(states.LocalPath, FileMode.Create, FileAccess.Write, FileShare.Write);
        fileStream.SetLength(fileSize);

        // Initialize workers
        int numberOfWorkers = (int)Math.Min(WorkersPerDownloadTask, totalChunks);
        Task[] workers = new Task[numberOfWorkers];
        for (int i = 0; i < numberOfWorkers; i++) {
            workers[i] = MultipartDownloadWorker(states, request, cancellationToken);
        }
        await Task.WhenAll(workers);
    }

    private async Task MultipartDownloadWorker(DownloadStates states, DownloadRequest downloadRequest, CancellationToken cancellationToken = default) {
        using var fileStream = new FileStream(states.LocalPath, FileMode.Open, FileAccess.Write, FileShare.Write);

        byte[] downloadBufferArr = ArrayPool<byte>.Shared.Rent(DOWNLOAD_BUFFER_SIZE);
        Memory<byte> downloadBuffer = downloadBufferArr.AsMemory(0, DOWNLOAD_BUFFER_SIZE);

        while (states.NextChunk() is (long start, long end)) {
            fileStream.Seek(start, SeekOrigin.Begin);

            var response = await states.Url.WithHeader("Range", $"bytes={start}-{end}").GetAsync(cancellationToken: cancellationToken);
            response.ResponseMessage.EnsureSuccessStatusCode();

            using var contentStream = await response.GetStreamAsync();
            await WriteStreamToFile(contentStream, fileStream, downloadBuffer, downloadRequest, cancellationToken);
        }

        ArrayPool<byte>.Shared.Return(downloadBufferArr);
    }

    private async Task DownloadFileInGroupAsync(DownloadRequest request, GroupDownloadRequest groupRequest, Dictionary<DownloadRequest, DownloadResult> failed, CancellationToken cancellationToken) {
        DownloadResult result = await DownloadFileAsync(request, cancellationToken);
        if (result.Type == DownloadResultType.Failed) {
            failed.Add(request, result);
        }

        groupRequest.SingleRequestCompleted?.Invoke(request, result);
    }
}