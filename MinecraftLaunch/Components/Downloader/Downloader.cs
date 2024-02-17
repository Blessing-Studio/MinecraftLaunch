using Flurl.Http;
using MinecraftLaunch.Classes.Interfaces;
using MinecraftLaunch.Classes.Models.Download;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Net;
using System.Net.Sockets;
using System.Timers;
using static System.Runtime.InteropServices.JavaScript.JSType;
using DownloadProgressChangedEventArgs = MinecraftLaunch.Classes.Models.Event.DownloadProgressChangedEventArgs;
using Timer = System.Timers.Timer;

namespace MinecraftLaunch.Components.Downloader;

public sealed class Downloader : IDownloader, IDisposable
{
    public bool IsDisposed { get; private set; } = false;
    public int MaxRetryCount { get; set; } = 4;
    public int ChunkCount { get; set; } = 4;
    public ThreadPoolImplement ThreadPool { get; private set; }
    public HttpClient Client { get; set; } = new();
    public sealed class ThreadPoolImplement : IDisposable
    {
        private int _threadCount = 0;
        public bool IsDisposed { get; private set; } = false;
        public int ThreadCount
        {
            get
            {
                return _threadCount;
            }
            set
            {
                if (value < 0) throw new ArgumentException();
                if (value > _threadCount)
                {
                    while (_threads.Count != value)
                    {
                        CancellationTokenSource cancellationTokenSource = new();
                        CancellationToken token = cancellationTokenSource.Token;
                        Thread thread = null;
                        thread = new(() =>
                        {
                            while (!token.IsCancellationRequested)
                            {
                                if (_firstTasks.IsEmpty && _tasks.IsEmpty)
                                {
                                    Thread.Sleep((FreeThreadCount + ThreadCount / 2) / (ThreadCount + 1) * 64);
                                    continue;
                                }
                                lock (_freeThreadCountLock)
                                    FreeThreadCount--;
                                thread.Name = "ThreadPool Thread - Running";
                                try
                                {
                                    if (_firstTasks.TryDequeue(out Action action))
                                    {
                                        action.Invoke();
                                    }
                                    else if (_tasks.TryDequeue(out Action action2))
                                    {
                                        action2.Invoke();
                                    }
                                }
                                catch { }
                                lock (_freeThreadCountLock)
                                    FreeThreadCount++;
                                thread.Name = "ThreadPool Thread";
                            }
                        });
                        _threads[cancellationTokenSource] = thread;
                        lock (_freeThreadCountLock)
                            FreeThreadCount++;
                        thread.Name = "ThreadPool Thread";
                        thread.IsBackground = true;
                        thread.Start();
                    }
                }
                else if (value < _threadCount)
                {
                    List<Thread> toJoin = new();
                    while (_threads.Count != ThreadCount)
                    {
                        KeyValuePair<CancellationTokenSource, Thread> pair = _threads.TakeLast(1).First();
                        pair.Key.Cancel();
                        toJoin.Add(pair.Value);
                        lock (_freeThreadCountLock)
                            FreeThreadCount--;
                    }
                    foreach (Thread thread in toJoin) thread.Join(TimeSpan.FromSeconds(2));
                }
                _threadCount = value;
            }
        }
        private object _freeThreadCountLock = new();
        public int FreeThreadCount { get; private set; }
        private ConcurrentDictionary<CancellationTokenSource, Thread> _threads = new();
        private ConcurrentQueue<Action> _tasks = new();
        private ConcurrentQueue<Action> _firstTasks = new();
        ~ThreadPoolImplement()
        {
            Dispose();
        }
        public ThreadPoolImplement(int maxThreadCount)
        {
            ThreadCount = maxThreadCount;
        }
        public void CancelAll()
        {
            _tasks.Clear();
            _firstTasks.Clear();
        }
        public void WaitAll()
        {
            while (FreeThreadCount != ThreadCount || !_tasks.IsEmpty || !_firstTasks.IsEmpty) Thread.Sleep(16);
        }
        public void RunSync(Action action)
        {
            int _SpinLock = 1;
            while (Interlocked.Exchange(ref _SpinLock, 1) != 0)
            {
                Thread.SpinWait(1);
            }
            _tasks.Enqueue(() =>
            {
                action();
                Interlocked.Exchange(ref _SpinLock, 0);
            });
        }
        public void RunSync<T>(Action<T> action, T arg1)
        {
            int _SpinLock = 1;
            while (Interlocked.Exchange(ref _SpinLock, 1) != 0)
            {
                Thread.SpinWait(1);
            }
            _tasks.Enqueue(() =>
            {
                action(arg1);
                Interlocked.Exchange(ref _SpinLock, 0);
            });
        }
        public void RunSync<T1, T2>(Action<T1, T2> action, T1 arg1, T2 arg2)
        {
            int _SpinLock = 1;
            while (Interlocked.Exchange(ref _SpinLock, 1) != 0)
            {
                Thread.SpinWait(1);
            }
            _tasks.Enqueue(() =>
            {
                action(arg1, arg2);
                Interlocked.Exchange(ref _SpinLock, 0);
            });
        }
        public void RunSync<T1, T2, T3>(Action<T1, T2, T3> action, T1 arg1, T2 arg2, T3 arg3)
        {
            int _SpinLock = 1;
            while (Interlocked.Exchange(ref _SpinLock, 1) != 0)
            {
                Thread.SpinWait(1);
            }
            _tasks.Enqueue(() =>
            {
                action(arg1, arg2, arg3);
                Interlocked.Exchange(ref _SpinLock, 0);
            });
        }
        public void RunSync<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            int _SpinLock = 1;
            while (Interlocked.Exchange(ref _SpinLock, 1) != 0)
            {
                Thread.SpinWait(1);
            }
            _tasks.Enqueue(() =>
            {
                action(arg1, arg2, arg3, arg4);
                Interlocked.Exchange(ref _SpinLock, 0);
            });
        }
        public void Run(Action action)
        {
            _tasks.Enqueue(() =>
            {
                action();
            });
        }
        public void Run<T>(Action<T> action, T arg1)
        {
            _tasks.Enqueue(() =>
            {
                action(arg1);
            });
        }
        public void Run<T1, T2>(Action<T1, T2> action, T1 arg1, T2 arg2)
        {
            _tasks.Enqueue(() =>
            {
                action(arg1, arg2);
            });
        }
        public void Run<T1, T2, T3>(Action<T1, T2, T3> action, T1 arg1, T2 arg2, T3 arg3)
        {
            _tasks.Enqueue(() =>
            {
                action(arg1, arg2, arg3);
            });
        }
        public void Run<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            _tasks.Enqueue(() =>
            {
                action(arg1, arg2, arg3, arg4);
            });
        }
        public void RunFirst(Action action)
        {
            _firstTasks.Enqueue(() =>
            {
                action();
            });
        }
        public void Dispose()
        {
            if (!IsDisposed)
            {
                CancelAll();
                ThreadCount = 0;
                GC.SuppressFinalize(this);
            }
        }
    }
    
    public event Action<bool> Completed;
    public event EventHandler<DownloadProgressChangedEventArgs> ProgressChanged;

    private readonly Timer _timer;
    private IEnumerable<DownloadRequest> _downloadItems;
    private ArrayPool<byte> _buffer { get; } = ArrayPool<byte>.Create();

    private int _totalBytes;
    private int _downloadedBytes;
    private int _previousDownloadedBytes;

    private int _totalCount;
    private int _completedCount;
    private int _failedCount;
    private int _chunkCount;
    ~Downloader()
    {
        Dispose();
    }
    public Downloader(int threadCount)
    {
        Client.Timeout = TimeSpan.FromSeconds(15);
        Client.DefaultRequestHeaders.Connection.Add("keep-alive");
        ServicePointManager.DefaultConnectionLimit = 512;
        _timer = new Timer
        {
            Interval = TimeSpan.FromSeconds(1).TotalMilliseconds
        };

        _timer.Elapsed += (sender, e) => UpdateDownloadProgress();
        ThreadPool = new(threadCount);
    }
    public Downloader() : this(128) { }

    public void Cancel()
    {
        _timer.Stop();
        ThreadPool.CancelAll();
    }

    public bool Download()
    {
        _timer.Start();
        int retryCount = 0;
        while (retryCount < MaxRetryCount)
        {
            foreach (DownloadRequest downloadRequest in _downloadItems)
            {
                if (downloadRequest.IsCompleted) continue;
                ThreadPool.Run(() =>
                {
                    if (!DownloadItem(downloadRequest))
                    {
                        Console.WriteLine(downloadRequest.FileInfo.Name);
                        Interlocked.Increment(ref _failedCount);
                    }
                    else
                    {
                        Interlocked.Increment(ref _completedCount);
                    }
                });
            }
            ThreadPool.WaitAll();
            if (_failedCount > 0)
            {
                _downloadItems = _downloadItems.Where(x => !x.IsCompleted).ToList();
                _failedCount = 0;
                retryCount++;
                Thread.Sleep(16000);
            }
            else
            {
                break;
            }
        }
        Completed?.Invoke(retryCount == 0);
        _timer.Stop();
        return retryCount == 0;
    }

    public async ValueTask<bool> DownloadAsync()
    {
        return await Task.Run(() => Download());
    }

    public bool DownloadItem(DownloadRequest downloadRequest, bool forceNotUsePartial = false)
    {
        downloadRequest.IsPartialContentSupported = GetIsPartialContentSupported(downloadRequest.Url);
        if (downloadRequest.IsPartialContentSupported && downloadRequest.Size >= 1024 * 1024 && !forceNotUsePartial)
        {
            using ThreadPoolImplement pool = new(ChunkCount);
            using MemoryStream result = new(downloadRequest.Size);
            int chunkSize = (int)Math.Ceiling((double)downloadRequest.Size / ChunkCount);
            bool failed = false;
            for (int i = 0; i < (downloadRequest.Size / chunkSize); i++)
            {
                pool.Run(() =>
                {
                    failed |= !DownloadPart(downloadRequest, result, i, chunkSize);
                });
            }
            pool.WaitAll();
            if (!failed)
            {
                downloadRequest.FileInfo.Directory.Create();
                File.WriteAllBytes(downloadRequest.FileInfo.FullName, result.ToArray());
                downloadRequest.IsCompleted = true;
                Interlocked.Add(ref _downloadedBytes, downloadRequest.Size);
                downloadRequest.DownloadedBytes = downloadRequest.Size;
                return true;
            }
            return DownloadItem(downloadRequest, true);
        }
        else
        {
            bool failed = !DownloadSingleFile(downloadRequest);
            if (failed)
            {
                return false;
            }
            downloadRequest.IsCompleted = true;
            Interlocked.Add(ref _downloadedBytes, downloadRequest.Size);
            downloadRequest.DownloadedBytes = downloadRequest.Size;
            return true;
        }
    }

    public void Retry()
    {
        Cancel();
        Setup(_downloadItems.Where(x => !x.IsCompleted));
        Download();
    }

    public void Setup(IEnumerable<DownloadRequest> downloadItems)
    {
        // Initialize states
        _downloadItems = downloadItems.ToImmutableList();
        _totalBytes = _downloadItems.Sum(item => item.Size);
        _downloadedBytes = 0;
        _previousDownloadedBytes = 0;

        _totalCount = _downloadItems.Count();
        _completedCount = 0;
        _failedCount = 0;
    }

    public void Dispose()
    {
        if (!IsDisposed)
        {
            _timer.Dispose();
            Client.Dispose();
            ThreadPool.Dispose();
            GC.SuppressFinalize(this);
        }
    }
    private void UpdateDownloadProgress()
    {
        int diffBytes = _downloadedBytes - _previousDownloadedBytes;
        _previousDownloadedBytes = _downloadedBytes;

        var progress = new DownloadProgressChangedEventArgs
        {
            TotalCount = _totalCount,
            CompletedCount = _completedCount,
            FailedCount = _failedCount,
            TotalBytes = _totalBytes,
            DownloadedBytes = _downloadedBytes,
            Speed = diffBytes / 1,
        };

        ProgressChanged?.Invoke(this, progress);
    }
    private bool DownloadPart(DownloadRequest downloadRequest, Stream result, int i, int chunkSize)
    {
        try
        {
            using HttpRequestMessage message = new()
            {
                RequestUri = new(downloadRequest.Url)
            };
            message.Headers.Range = new(i * chunkSize, Math.Min(((i + 1) * chunkSize) - 1, downloadRequest.Size));
            using HttpResponseMessage response = Client.Send(message);
            if (!response.IsSuccessStatusCode) return false;
            byte[] data = response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
            lock (result)
            {
                result.Position = i * chunkSize;
                result.Write(data);
            }
            return true;
        }
        catch
        {
            return false;
        }
    }
    private bool DownloadSingleFile(DownloadRequest downloadRequest)
    {
        try
        {
            DownloadExtensions.DownloadFileAsync(new FlurlRequest(downloadRequest.Url), downloadRequest.FileInfo.Directory.FullName, downloadRequest.FileInfo.Name).GetAwaiter().GetResult();
            //using HttpRequestMessage message = new()
            //{
            //    RequestUri = new(downloadRequest.Url)
            //};
            //using HttpResponseMessage response = Client.Send(message);
            //if (!response.IsSuccessStatusCode) return false;
            //using Stream stream = response.Content.ReadAsStream();
            //int size = 1;
            //downloadRequest.FileInfo.Directory.Create();
            //using FileStream fileStream = new(downloadRequest.FileInfo.FullName, FileMode.OpenOrCreate);
            //byte[] buffer = _buffer.Rent(4096);
            //while (size > 0)
            //{
            //    size = stream.Read(buffer);
            //    fileStream.Write(buffer, 0, size);
            //}
            //_buffer.Return(buffer);
            //downloadRequest.IsCompleted = true;
            return true;
        }
        catch { return false; }
    }
    private async Task<bool> GetIsPartialContentSupportedAsync(string url)
    {
        using var request = new HttpRequestMessage(HttpMethod.Head, url);
        using var response = await new HttpClient().SendAsync(request);
        return response.Headers.AcceptRanges.Contains("bytes");
    }

    private bool GetIsPartialContentSupported(string url)
    {
        using var request = new HttpRequestMessage(HttpMethod.Head, url);
        using var response = new HttpClient().Send(request);
        return response.Headers.AcceptRanges.Contains("bytes");
    }
}
