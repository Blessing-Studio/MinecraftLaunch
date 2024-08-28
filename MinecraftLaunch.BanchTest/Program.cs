using BenchmarkDotNet.Running;
using BenchmarkDotNet.Attributes;
using MinecraftLaunch.Components.Checker;
using MinecraftLaunch.Components.Resolver;
using MinecraftLaunch.Extensions;
using MinecraftLaunch.Classes.Interfaces;
using System.Threading.Tasks.Dataflow;
using MinecraftLaunch.Utilities;
using System.Diagnostics;

//var summary = BenchmarkRunner.Run<BenchmarkClass>();

await new BenchmarkClass().RunTaskParallel();
Console.ReadKey();

[MemoryDiagnoser]
public class BenchmarkClass {
    private readonly ResourceChecker _checker;
    private readonly GameResolver _gameResolver = new("C:\\Users\\w\\Desktop\\temp\\.minecraft");

    public BenchmarkClass() {
        _checker = new(_gameResolver.GetGameEntity("1.16.5"));
        _ = _checker.CheckAsync();
    }

    [Benchmark]
    public ValueTask<bool> RunTaskParallel() {
        return _checker.MissingResources.DownloadResourceParallel();
    }

    [Benchmark]
    public ValueTask<bool> RunParallel() {
        return _checker.MissingResources.DownloadResourceEntrysAsync();
    }
}

public static class Downloader {
    public static async ValueTask<bool> DownloadResourceParallel(this IEnumerable<IDownloadEntry> entries) {
        int completedCount = 0;
        int totalCount = entries.Count();

        TransformBlock<IDownloadEntry, IDownloadEntry> transformBlock = new(x => {
            return x;
        }, new ExecutionDataflowBlockOptions {
            BoundedCapacity = DownloadUitl.DefaultDownloadRequest.MultiThreadsCount,
            MaxDegreeOfParallelism = DownloadUitl.DefaultDownloadRequest.MultiThreadsCount,
        });

        ActionBlock<IDownloadEntry> actionBlock = new(async x => {
            await Task.Run(async () => {
                if (string.IsNullOrEmpty(x.Url)) {
                    return;
                }

                await DownloadUitl.DownloadAsync(x).AsTask().ContinueWith(task => {
                    if (task.IsFaulted) {
                        if (!x.Verify()) {
                            Debug.WriteLine(task.Exception.Message);
                            return;
                        }

                        return;
                    }

                    var downloadResult = task.Result;
                });

                Interlocked.Increment(ref completedCount);
            });
        }, new ExecutionDataflowBlockOptions {
            BoundedCapacity = DownloadUitl.DefaultDownloadRequest.MultiThreadsCount,
            MaxDegreeOfParallelism = DownloadUitl.DefaultDownloadRequest.MultiThreadsCount,
        });

        var transformManyBlock = new TransformManyBlock<IEnumerable<IDownloadEntry>, IDownloadEntry>(chunk => chunk,
            new ExecutionDataflowBlockOptions());

        var linkOptions = new DataflowLinkOptions {
            PropagateCompletion = true
        };

        transformManyBlock.LinkTo(transformBlock, linkOptions);
        transformBlock.LinkTo(actionBlock, linkOptions);

        if (entries != null) {
            transformManyBlock.Post(entries);
        }

        transformManyBlock.Complete();
        await actionBlock.Completion.WaitAsync(new CancellationToken());
        return true;
    }
}