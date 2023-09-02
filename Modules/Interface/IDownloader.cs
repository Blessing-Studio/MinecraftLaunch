using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MinecraftLaunch.Modules.Models;

public interface IDownloader : IDisposable
{
    event EventHandler<Exception> DownloadFailed;

    Stopwatch DownloadTimeStopwatch { get; }

    int ReadBufferSize { get; set; }

    void BeginDownload();
}

public interface IDownloaderProgressProvider<TProgress>
{
    event EventHandler<TProgress> DownloadProgressChanged;
}

public interface IDownloader<TResult, TProgress> : IDownloader, IDownloaderProgressProvider<TProgress>
{
    event EventHandler<TResult> DownloadCompleted;

    Task<TResult> DownloadProcess { get; }

    ValueTask<TResult> CompleteAsync();
}

public abstract class DownloaderBase<TResult, TProgress> : IDownloader<TResult, TProgress>
{
    public event EventHandler<TResult> DownloadCompleted;

    public event EventHandler<Exception> DownloadFailed;

    public event EventHandler<TProgress> DownloadProgressChanged;

    public Stopwatch DownloadTimeStopwatch { get; protected set; } = new Stopwatch();

    public Task<TResult> DownloadProcess { get; protected set; }

    public int ReadBufferSize { get; set; } = 1024 * 1024;

    public virtual void BeginDownload() => DownloadTimeStopwatch.Start();

    public virtual async ValueTask<TResult> CompleteAsync() => await DownloadProcess;

    protected virtual void OnProgressChanged(TProgress progress) => DownloadProgressChanged?.Invoke(this, progress);

    protected virtual void OnDownloadFailed(Exception exception) => DownloadFailed?.Invoke(this, exception);

    protected virtual void OnDownloadCompleted(TResult result) => DownloadCompleted?.Invoke(this, result);

    public virtual void Dispose()
    {

    }
}
