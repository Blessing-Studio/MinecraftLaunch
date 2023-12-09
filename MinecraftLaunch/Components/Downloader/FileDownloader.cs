using Flurl.Http;
using System.Net;
using System.Buffers;
using System.Collections.Immutable;
using System.Threading.Tasks.Dataflow;
using MinecraftLaunch.Classes.Models.Download;

using Timer = System.Timers.Timer;
using DownloadProgressChangedEventArgs = MinecraftLaunch.Classes.Models.Event.DownloadProgressChangedEventArgs;

namespace MinecraftLaunch.Components.Downloader {
    public class FileDownloader {
        private int _totalBytes;

        private int _totalCount;

        private int _failedCount;

        private int _completedCount;

        private int _downloadedBytes;

        private readonly Timer _timer;

        private const int BUFFER_SIZE = 4096;

        private int _previousDownloadedBytes;

        private const int MAX_RETRY_COUNT = 3;

        private CancellationTokenSource _userCts;

        private const double UPDATE_INTERVAL = 1.0;

        private readonly ArrayPool<byte> _bufferPool;

        private readonly AutoResetEvent _autoResetEvent;

        private ImmutableList<DownloadRequest> _downloadRequests;

        private readonly ExecutionDataflowBlockOptions _parallelOptions;

        public event Action<bool> Completed;

        public event EventHandler<DownloadProgressChangedEventArgs> ProgressChanged;

        public FileDownloader(DownloadRequest downloadRequest) {
            List<DownloadRequest> temp = [downloadRequest];
            _downloadRequests = temp.ToImmutableList();

            _parallelOptions = new ExecutionDataflowBlockOptions {
                MaxDegreeOfParallelism = Environment.ProcessorCount * 2,
            };
            _autoResetEvent = new AutoResetEvent(true);
            _bufferPool = ArrayPool<byte>.Create(BUFFER_SIZE,
                Environment.ProcessorCount * 2);

            _timer = new() {
                Interval = UPDATE_INTERVAL
            };

            Init();
        }

        public FileDownloader(IEnumerable<DownloadRequest> downloadRequests) {
            _downloadRequests = downloadRequests.ToImmutableList();

            _parallelOptions = new ExecutionDataflowBlockOptions {
                MaxDegreeOfParallelism = Environment.ProcessorCount * 2,
            };
            _autoResetEvent = new AutoResetEvent(true);
            _bufferPool = ArrayPool<byte>.Create(BUFFER_SIZE,
                Environment.ProcessorCount * 2);

            _timer = new() {
                Interval = 250
            };

            Init();
        }

        public async ValueTask<bool> StartAsync() {
            while (true) {
                _timer.Start();

                try {
                    await ProcessDownloadRequests();
                }
                catch (OperationCanceledException) {
                    // Log the error
                    //Console.WriteLine("Operation was canceled.");
                }

                _timer.Stop();
                ReportDownloadProgress();

                if (IsDownloadCompleted()) {
                    InvokeCompleted(true);
                    return true;
                }

                CleanIncompleteFiles();

                if (IsDownloadFailed()) {
                    InvokeCompleted(false);
                }

                WaitForRetryOrCancel();

                if (IsCanceled()) {
                    InvokeCompleted(false);
                    return false;
                }
            }
        }

        private async Task ProcessDownloadRequests() {
            var downloader = new ActionBlock<DownloadRequest>(async item => {
                await DownloadItemWithRetry(item);
            }, _parallelOptions);

            PostDownloadRequests(downloader);

            downloader.Complete();
            await downloader.Completion;
        }

        private async Task DownloadItemWithRetry(DownloadRequest item) {
            for (int i = 0; i < MAX_RETRY_COUNT && !_userCts.IsCancellationRequested; i++) {
                if (await DownloadAsync(item)) {
                    break;
                }
            }
        }

        private void PostDownloadRequests(ActionBlock<DownloadRequest> downloader) {
            foreach (var item in _downloadRequests) {
                downloader.Post(item);
            }
        }

        private bool IsDownloadCompleted() {
            return _completedCount == _totalCount;
        }

        private void CleanIncompleteFiles() {
            foreach (var item in _downloadRequests) {
                if (!item.IsCompleted && File.Exists(item.Path)) {
                    File.Delete(item.Path);
                }
            }
        }

        private bool IsDownloadFailed() {
            return _failedCount > 0 && !_userCts.IsCancellationRequested;
        }

        private void WaitForRetryOrCancel() {
            _autoResetEvent.WaitOne();
        }

        private bool IsCanceled() {
            return _userCts.IsCancellationRequested;
        }

        private void InvokeCompleted(bool result) {
            Completed?.Invoke(result);
        }

        private void Init() {
            _userCts = new CancellationTokenSource();
            _parallelOptions.CancellationToken = _userCts.Token;
            _totalBytes = _downloadRequests.Sum(item => item.Size);
            _downloadedBytes = 0;
            _previousDownloadedBytes = 0;

            _totalCount = _downloadRequests.Count();
            _completedCount = 0;
            _failedCount = 0;

            if (_userCts.IsCancellationRequested) {
                _userCts.Dispose();
                _userCts = new CancellationTokenSource();
                _parallelOptions.CancellationToken = _userCts.Token;
            }

            _autoResetEvent.Reset();


            _timer.Elapsed += (sender, e) => ReportDownloadProgress();
        }

        private void ReportDownloadProgress() {
            // Calculate speed
            int diffBytes = _downloadedBytes - _previousDownloadedBytes;
            _previousDownloadedBytes = _downloadedBytes;

            var progress = new DownloadProgressChangedEventArgs {
                TotalCount = _totalCount,
                CompletedCount = _completedCount,
                FailedCount = _failedCount,
                TotalBytes = _totalBytes,
                DownloadedBytes = _downloadedBytes,
                Speed = diffBytes / UPDATE_INTERVAL,
            };

            ProgressChanged?.Invoke(this, progress);
        }

        private async ValueTask<bool> DownloadAsync(DownloadRequest item) {
            if (_userCts.IsCancellationRequested) {
                return true;
            }

            // Ensure the directory exists
            EnsureDirectoryExists(item.Path);

            byte[] buffer = _bufferPool.Rent(BUFFER_SIZE);

            try {
                if (string.IsNullOrEmpty(item.Url)) {
                    MarkAsCompleted(item);
                    return true;
                }

                var response = await GetResponseAsync(item.Url);

                if (response.StatusCode == HttpStatusCode.Found) {
                    // Handle redirection
                    response = await GetResponseAsync(response.Headers.Location!.ToString());
                }

                if (item.Size == 0) {
                    UpdateItemSize(response, item);
                }

                item.IsPartialContentSupported = response.Headers.AcceptRanges.Contains("bytes");

                await using var httpStream = await response.Content.ReadAsStreamAsync();
                await using var fileStream = File.OpenWrite(item.Path);

                var timeout = TimeSpan.FromSeconds(Math.Max(item.Size / 16384.0, 30.0));
                using var timeoutCts = new CancellationTokenSource(timeout);
                using var readCts = CancellationTokenSource.CreateLinkedTokenSource(_userCts.Token, timeoutCts.Token);

                await DownloadFileAsync(httpStream, fileStream, buffer, readCts.Token, item);

                // Download successful
                MarkAsCompleted(item);

                response.Dispose();
                return true;
            }
            catch (Exception ex) {
                // Log the error
                Console.WriteLine(ex);
            }
            finally {
                GC.Collect();
            }

            // If is not caused by cancellation, mark as failure
            if (!_userCts.IsCancellationRequested) {
                MarkAsFailed(item);
            }

            return false; // We're not done yet, prepare for retry
        }

        private void EnsureDirectoryExists(string path) {
            if (Path.IsPathRooted(path)) {
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            }
        }

        private async Task<HttpResponseMessage> GetResponseAsync(string url) {
            return (await url.GetAsync(cancellationToken: _userCts.Token)).ResponseMessage;
        }

        private void UpdateItemSize(HttpResponseMessage response, DownloadRequest item) {
            item.Size = (int)(response.Content.Headers.ContentLength ?? 0);
            Interlocked.Add(ref _totalBytes, item.Size);
        }

        private async Task DownloadFileAsync(Stream httpStream, Stream fileStream, byte[] buffer, CancellationToken token, DownloadRequest item) {
            int bytesReceived;
            while ((bytesReceived = await httpStream.ReadAsync(buffer, token)) > 0) {
                fileStream.Write(buffer, 0, bytesReceived);
                item.DownloadedBytes += bytesReceived;
                Interlocked.Add(ref _downloadedBytes, bytesReceived);
            }
        }

        private void MarkAsCompleted(DownloadRequest item) {
            item.IsCompleted = true;
            Interlocked.Increment(ref _completedCount);
        }

        private void MarkAsFailed(DownloadRequest item) {
            Interlocked.Increment(ref _failedCount);
            Interlocked.Add(ref _downloadedBytes, -item.DownloadedBytes);
            item.DownloadedBytes = 0;
            Interlocked.Exchange(ref _previousDownloadedBytes, _downloadedBytes);
        }
    }
}
