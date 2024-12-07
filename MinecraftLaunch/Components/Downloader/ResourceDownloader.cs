using MinecraftLaunch.Classes.Interfaces;
using MinecraftLaunch.Classes.Models.Download;
using MinecraftLaunch.Classes.Models.Event;
using MinecraftLaunch.Extensions;
using MinecraftLaunch.Utilities;
using System.Diagnostics;
using System.Threading.Tasks.Dataflow;

namespace MinecraftLaunch.Components.Downloader;

public class ResourceDownloader(
    DownloadRequest request,
    IEnumerable<IDownloadEntry> downloadEntries,
    MirrorDownloadSource downloadSource = default,
    CancellationTokenSource tokenSource = default)  {

    public event EventHandler<DownloadProgressChangedEventArgs> ProgressChanged;

    public async ValueTask<bool> DownloadAsync() {
        int completedCount = 0;
        int totalCount = downloadEntries.Count();

        var transformBlock = new TransformBlock<IDownloadEntry, IDownloadEntry>(e => {
            if (string.IsNullOrEmpty(e.Url)) {
                return e;
            }

            if (MirrorDownloadManager.IsUseMirrorDownloadSource) {
                downloadEntries = downloadEntries.Select(x => x.OfMirrorSource(downloadSource));
            }

            return e;
        }, new ExecutionDataflowBlockOptions {
            BoundedCapacity = request.MultiThreadsCount,
            MaxDegreeOfParallelism = request.MultiThreadsCount,
            CancellationToken = tokenSource is null ? default : tokenSource.Token
        });

        var actionBlock = new ActionBlock<IDownloadEntry>(async e => {
            if (string.IsNullOrEmpty(e.Url)) {
                return;
            }

            await DownloadUitl.DownloadAsync(e, tokenSource: tokenSource).AsTask().ContinueWith(task => {
                if (task.IsFaulted) {
                    if (!e.Verify()) {
                        Debug.WriteLine(task.Exception.Message);
                        return;
                    }

                    return;
                }

                var downloadResult = task.Result;
            });

            Interlocked.Increment(ref completedCount);
            ProgressChanged?.Invoke(this, new DownloadProgressChangedEventArgs {
                TotalCount = totalCount,
                CompletedCount = completedCount
            });
        },
        new ExecutionDataflowBlockOptions {
            BoundedCapacity = request.MultiThreadsCount,
            MaxDegreeOfParallelism = request.MultiThreadsCount,
            CancellationToken = tokenSource is null ? default : tokenSource.Token
        });

        var transformManyBlock = new TransformManyBlock<IEnumerable<IDownloadEntry>, IDownloadEntry>(chunk => chunk,
            new ExecutionDataflowBlockOptions());

        var linkOptions = new DataflowLinkOptions {
            PropagateCompletion = true
        };

        transformManyBlock.LinkTo(transformBlock, linkOptions);
        transformBlock.LinkTo(actionBlock, linkOptions);

        if (downloadEntries != null) {
            transformManyBlock.Post(downloadEntries);
        }

        transformManyBlock.Complete();
        await actionBlock.Completion.WaitAsync(tokenSource is null ? default : tokenSource.Token);
        return true;
    }
}