using System.Net;
using System.Buffers;
using System.Net.Http.Headers;
using Timer = System.Timers.Timer;
using System.Collections.Immutable;
using System.Threading.Tasks.Dataflow;
using MinecraftLaunch.Classes.Interfaces;
using MinecraftLaunch.Classes.Models.Download;
using DownloadProgressChangedEventArgs = MinecraftLaunch.Classes.Models.Event.DownloadProgressChangedEventArgs;

namespace MinecraftLaunch.Components.Downloader;

[Obsolete]
public sealed class BatchDownloader {
    private const int MAX_RETRY_COUNT = 3;
    private const int BUFFER_SIZE = 4096; // byte
    private const int SIZE_THRESHOLD = 1048576;
    private const double UPDATE_INTERVAL = 1.0; // second

    private readonly HttpClient _client;
    private readonly ArrayPool<byte> _bufferPool;
    private readonly ExecutionDataflowBlockOptions _parallelOptions;
    private readonly Timer _timer;
    private readonly AutoResetEvent _autoResetEvent;

    private CancellationTokenSource _userCts;
    private ImmutableList<DownloadRequest> _downloadItems;

    private int _totalBytes;
    private int _downloadedBytes;
    private int _previousDownloadedBytes;

    private int _totalCount;
    private int _completedCount;
    private int _failedCount;
    private int _chunkCount;

    private bool _useChunkedDownload;

    public event Action<bool> Completed;

    public event EventHandler<DownloadProgressChangedEventArgs> ProgressChanged;

    //public BatchDownloader(int chunkCount = 8, bool useChunkedDownload = true) {
    //    _client = new HttpClient();
    //    _client.DefaultRequestHeaders.Connection.Add("keep-alive");

    //    _chunkCount = chunkCount;
    //    _bufferPool = ArrayPool<byte>.Create(BUFFER_SIZE, Environment.ProcessorCount * 2);
    //    _autoResetEvent = new AutoResetEvent(true);

    //    _parallelOptions = new ExecutionDataflowBlockOptions {
    //        MaxDegreeOfParallelism = Environment.ProcessorCount * 2,
    //    };

    //    _userCts = new CancellationTokenSource();
    //    _parallelOptions.CancellationToken = _userCts.Token;

    //    _timer = new Timer {
    //        Interval = TimeSpan.FromSeconds(UPDATE_INTERVAL).TotalMilliseconds
    //    };

    //    _timer.Elapsed += (sender, e) => UpdateDownloadProgress();
    //}

    //public void Setup(IEnumerable<DownloadRequest> downloadItems) {
    //    // Initialize states
    //    _downloadItems = downloadItems.ToImmutableList();
    //    _totalBytes = _downloadItems.Sum(item => item.Size);
    //    _downloadedBytes = 0;
    //    _previousDownloadedBytes = 0;

    //    _totalCount = _downloadItems.Count();
    //    _completedCount = 0;
    //    _failedCount = 0;

    //    if (_userCts.IsCancellationRequested) {
    //        _userCts.Dispose();
    //        _userCts = new CancellationTokenSource();
    //        _parallelOptions.CancellationToken = _userCts.Token;
    //    }

    //    _autoResetEvent.Reset();
    //}

    //public void Retry() {
    //    _downloadItems = _downloadItems.Where(item => !item.IsCompleted).ToImmutableList();
    //    _failedCount = 0;
    //    _autoResetEvent.Set();
    //}

    //public void Cancel() {
    //    _userCts.Cancel();
    //    _timer.Stop();
    //    _autoResetEvent.Set();
    //}

    //public async ValueTask<bool> DownloadAsync() {
    //    while (true) {
    //        _timer.Start();

    //        try {
    //            var downloader = new ActionBlock<DownloadRequest>(async item =>
    //            {
    //                for (int i = 0; i < MAX_RETRY_COUNT && !_userCts.IsCancellationRequested; i++) {
    //                    if (await DownloadItemAsync(item, i)) {
    //                        break;
    //                    }
    //                }
    //            }, _parallelOptions);

    //            foreach (var item in _downloadItems) {
    //                downloader.Post(item);
    //            }

    //            downloader.Complete();
    //            await downloader.Completion;
    //        }
    //        catch (OperationCanceledException) {
    //            //_logService.Info(nameof(DownloadService), "Download canceled");
    //        }

    //        _timer.Stop();
    //        // Ensure the last progress report is fired
    //        UpdateDownloadProgress();

    //        // Succeeded
    //        if (_completedCount == _totalCount) {
    //            Completed?.Invoke(true);
    //            return true;
    //        }


    //        // Clean incomplete files
    //        foreach (var item in _downloadItems) {
    //            if (!item.IsCompleted && item.FileInfo.Exists) {
    //                item.FileInfo.Delete();
    //            }
    //        }

    //        if (_failedCount > 0 && !_userCts.IsCancellationRequested) {
    //            Completed?.Invoke(false);
    //        }

    //        // Wait for retry or cancel
    //        _autoResetEvent.WaitOne();

    //        // Canceled
    //        if (_userCts.IsCancellationRequested) {
    //            Completed?.Invoke(false);
    //            return false;
    //        }
    //    }
    //}

    //private void UpdateDownloadProgress() {
    //    //int diffBytes = _downloadedBytes - _previousDownloadedBytes;
    //    //_previousDownloadedBytes = _downloadedBytes;

    //    //var progress = new DownloadProgressChangedEventArgs {
    //    //    TotalCount = _totalCount,
    //    //    CompletedCount = _completedCount,
    //    //    FailedCount = _failedCount,
    //    //    TotalBytes = _totalBytes,
    //    //    DownloadedBytes = _downloadedBytes,
    //    //    Speed = diffBytes / UPDATE_INTERVAL,
    //    //};

    //    //ProgressChanged?.Invoke(this, progress);
    //}

    //private async ValueTask<bool> DownloadItemAsync(DownloadRequest item, int retryTimes) {
    //    if (_userCts.IsCancellationRequested) {
    //        return true;
    //    }

    //    // Make sure directory exists
    //    if (!item.FileInfo.Directory.Exists) {
    //        item.FileInfo.Directory.Create();
    //    }

    //    if (!item.FileInfo.Exists) {
    //        using var r = item.FileInfo.Create();
    //    }

    //    byte[] buffer = _bufferPool.Rent(BUFFER_SIZE);

    //    try {
    //        var request = new HttpRequestMessage(HttpMethod.Get, item.Url);
    //        var response =
    //            await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, _userCts.Token);

    //        if (response.StatusCode == HttpStatusCode.Found) {
    //            // Handle redirection
    //            request = new HttpRequestMessage(HttpMethod.Get, response.Headers.Location);
    //            response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead,
    //                _userCts.Token);
    //        }

    //        if (item.Size == 0) {
    //            item.Size = (int)(response.Content.Headers.ContentLength ?? 0);
    //            Interlocked.Add(ref _totalBytes, item.Size);
    //        }

    //        item.IsPartialContentSupported = response.Headers.AcceptRanges.Contains("bytes");

    //        // Calculate the size of each chunk
    //        int chunkSize = (int)Math.Ceiling((double)item.Size / _chunkCount);

    //        // Decide whether to use chunked download based on the size threshold
    //        bool useChunkedDownload = item.Size > SIZE_THRESHOLD && item.IsPartialContentSupported;

    //        for (int i = 0; i < (useChunkedDownload ? _chunkCount : 1); i++) {
    //            int chunkStart = i * chunkSize;
    //            int chunkEnd = useChunkedDownload ? Math.Min(chunkStart + chunkSize, item.Size) - 1 : item.Size - 1;

    //            request = new HttpRequestMessage(HttpMethod.Get, item.Url);
    //            request.Headers.Range = new RangeHeaderValue(chunkStart, chunkEnd);

    //            response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, _userCts.Token);

    //            await using var httpStream = await response.Content.ReadAsStreamAsync();
    //            await using var fileStream = new FileStream(item.FileInfo.FullName, FileMode.Open, FileAccess.Write, FileShare.Write);
    //            fileStream.Position = chunkStart;

    //            int bytesRead;
    //            while ((bytesRead = await httpStream.ReadAsync(buffer, _userCts.Token)) > 0) {
    //                await fileStream.WriteAsync(buffer, 0, bytesRead, _userCts.Token);
    //                item.DownloadedBytes += bytesRead;
    //                Interlocked.Add(ref _downloadedBytes, bytesRead);
    //            }
    //        }

    //        // Download successful
    //        item.IsCompleted = true;
    //        Interlocked.Increment(ref _completedCount);

    //        request.Dispose();
    //        response.Dispose();
    //        return true;
    //    }
    //    catch (OperationCanceledException) {
    //        if (!_userCts.IsCancellationRequested) {
    //            throw;
    //        }
    //    }
    //    catch (Exception) {
    //        throw;
    //    }
    //    finally {
    //        _bufferPool.Return(buffer);
    //    }

    //    if (!_userCts.IsCancellationRequested) {
    //        Interlocked.Increment(ref _failedCount);
    //        Interlocked.Add(ref _downloadedBytes, -item.DownloadedBytes);
    //        item.DownloadedBytes = 0;
    //        Interlocked.Exchange(ref _previousDownloadedBytes, _downloadedBytes);
    //    }

    //    return false;
    //}

    //void IDisposable.Dispose() {
    //    _client.Dispose();
    //    _userCts.Dispose();
    //    _autoResetEvent.Dispose();
    //}
}

