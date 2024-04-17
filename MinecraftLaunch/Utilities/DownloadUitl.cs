using Flurl.Http;
using System.Net;
using System.Buffers;
using System.Net.Http.Headers;
using System.Threading.Tasks.Dataflow;
using MinecraftLaunch.Classes.Interfaces;
using MinecraftLaunch.Classes.Models.Download;

using Timer = System.Timers.Timer;

namespace MinecraftLaunch.Utilities;

/// <summary>
/// 下载工具类
/// </summary>
public static class DownloadUitl {
    public static DownloadRequest DefaultDownloadRequest { get; set; } = new() {
        IsPartialContentSupported = true,
        FileSizeThreshold = 1024 * 1024 * 3,
        MultiThreadsCount = 64,
        MultiPartsCount = 8
    };

    public static async ValueTask<bool> DownloadAsync(
        DownloadRequest downloadRequest,
        CancellationTokenSource tokenSource = default,
        Action<double> perSecondProgressChangedAction = default) {
        Timer timer = default;
        downloadRequest ??= DefaultDownloadRequest;
        tokenSource ??= new CancellationTokenSource();
        perSecondProgressChangedAction ??= x => { };
        var responseMessage = (await downloadRequest.Url.GetAsync(cancellationToken: tokenSource.Token))
            .ResponseMessage;
        
        if (responseMessage.StatusCode.Equals(HttpStatusCode.Found)) {
            downloadRequest.Url = responseMessage.Headers.Location.AbsoluteUri;
            return await DownloadAsync(downloadRequest, tokenSource);
        }

        if (perSecondProgressChangedAction != null) {
            timer = new Timer(1000);
        }

        responseMessage.EnsureSuccessStatusCode();
        var contentLength = responseMessage.Content.Headers.ContentLength ?? 0;

        //use multipart download method
        if (downloadRequest.IsPartialContentSupported && contentLength > downloadRequest.FileSizeThreshold) {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, responseMessage.RequestMessage.RequestUri.AbsoluteUri);
            requestMessage.Headers.Range = new RangeHeaderValue(0, 1);

            return await MultiPartDownloadAsync(responseMessage, downloadRequest, downloadRequest.FileInfo.FullName, tokenSource)
                .AsTask()
                .ContinueWith(task => {
                    return !task.IsFaulted;
                });
        }

        return await WriteFileFromHttpResponseAsync(downloadRequest.FileInfo.FullName, responseMessage, tokenSource, (timer, perSecondProgressChangedAction, contentLength))
            .AsTask()
            .ContinueWith(task => {
                if (timer != null) {
                    try {
                        perSecondProgressChangedAction(responseMessage.Content.Headers.ContentLength != null ?
                            task.Result / (double)responseMessage.Content.Headers.ContentLength : 0);
                    } catch (Exception) {

                    }

                    timer.Stop();
                    timer.Dispose();
                }

                return !task.IsFaulted;
            });
    }

    public static async ValueTask<bool> DownloadAsync(
        IDownloadEntry downloadEntry,
        DownloadRequest downloadRequest = default,
        CancellationTokenSource tokenSource = default,
        Action<double> perSecondProgressChangedAction = default) {

        Timer timer = default;
        downloadRequest ??= DefaultDownloadRequest;
        tokenSource ??= new CancellationTokenSource();
        perSecondProgressChangedAction ??= x => { };
        var responseMessage = (await downloadEntry.Url.GetAsync(cancellationToken: tokenSource.Token))
            .ResponseMessage;

        if (responseMessage.StatusCode.Equals(HttpStatusCode.Found)) {
            downloadEntry.Url = responseMessage.Headers.Location.AbsoluteUri;
            return await DownloadAsync(downloadEntry, downloadRequest, tokenSource);
        }

        if (perSecondProgressChangedAction != null) {
            timer = new Timer(1000);
        }

        responseMessage.EnsureSuccessStatusCode();
        var contentLength = responseMessage.Content.Headers.ContentLength ?? 0;

        //use multipart download method
        if (downloadRequest.IsPartialContentSupported && contentLength > downloadRequest.FileSizeThreshold) {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, responseMessage.RequestMessage.RequestUri.AbsoluteUri);
            requestMessage.Headers.Range = new RangeHeaderValue(0, 1);

            return await MultiPartDownloadAsync(responseMessage, downloadRequest, downloadEntry.Path, tokenSource)
                .AsTask()
                .ContinueWith(task => {
                    return !task.IsFaulted;
                });
        }

        return await WriteFileFromHttpResponseAsync(downloadEntry.Path, responseMessage, tokenSource, (timer, perSecondProgressChangedAction, contentLength))
            .AsTask()
            .ContinueWith(task => {
                if (timer != null) {
                    try {
                        perSecondProgressChangedAction(responseMessage.Content.Headers.ContentLength != null ?
                            task.Result / (double)responseMessage.Content.Headers.ContentLength : 0);
                    } catch (Exception) {

                    }

                    timer.Stop();
                    timer.Dispose();
                }

                return !task.IsFaulted;
            });
    }

    private async static ValueTask<long> WriteFileFromHttpResponseAsync(
        string path, 
        HttpResponseMessage responseMessage, 
        CancellationTokenSource tokenSource, 
        (Timer, Action<double> perSecondProgressChangedAction, long?)? perSecondProgressChange = default) {
        var parentFolder = Path.GetDirectoryName(path);
        Directory.CreateDirectory(parentFolder);

        using var stream = await responseMessage.Content.ReadAsStreamAsync();
        using var fileStream = File.Create(path);
        using var rentMemory = MemoryPool<byte>.Shared.Rent(1024);

        long totalReadMemory = 0;
        int readMemory = 0;

        if (perSecondProgressChange != null) {
            var (timer, action, length) = perSecondProgressChange.Value;
            timer.Elapsed += (sender, e) => action(length != null ? (double)totalReadMemory / length.Value : 0);
            timer.Start();
        }

        while ((readMemory = await stream.ReadAsync(rentMemory.Memory, tokenSource.Token)) > 0) {
            await fileStream.WriteAsync(rentMemory.Memory[..readMemory], tokenSource.Token);
            Interlocked.Add(ref totalReadMemory, readMemory);
        }

        return totalReadMemory;
    }


    private static async ValueTask<long> MultiPartDownloadAsync(
        HttpResponseMessage responseMessage,
        DownloadRequest downloadSetting,
        string absolutePath,
        CancellationTokenSource tokenSource) {
        var requestMessage = new HttpRequestMessage(HttpMethod.Get, responseMessage.RequestMessage.RequestUri.AbsoluteUri);
        requestMessage.Headers.Range = new RangeHeaderValue(0, 1);

        var httpResponse = (await responseMessage.RequestMessage.RequestUri.GetAsync(cancellationToken:tokenSource.Token)).ResponseMessage;

        if (!httpResponse.IsSuccessStatusCode || httpResponse.Content.Headers.ContentLength.Value != 2)
            return await WriteFileFromHttpResponseAsync(absolutePath, responseMessage, tokenSource);

        var totalSize = responseMessage.Content.Headers.ContentLength.Value;
        var singleSize = totalSize / downloadSetting.MultiPartsCount;

        var rangesList = new List<MultiPartRange>();
        var folder = Path.GetDirectoryName(absolutePath);

        while (totalSize > 0) {
            bool enough = totalSize - singleSize > 1024 * 10;

            var range = new MultiPartRange {
                End = totalSize,
                Start = enough ? totalSize - singleSize : 0
            };

            range.TempFilePath = Path.Combine(folder, $"{range.Start}-{range.End}-" + Path.GetFileName(absolutePath));
            rangesList.Add(range);

            if (!enough) break;

            totalSize -= singleSize;
        }

        var transformBlock = new TransformBlock<MultiPartRange, (HttpResponseMessage, MultiPartRange)>(async range => {
            var message = (await responseMessage.RequestMessage.RequestUri.WithHeader("Range", $"bytes={range.Start}-{range.End}")
            .GetAsync(cancellationToken:tokenSource.Token)).ResponseMessage;
            return (message, range);
        }, new ExecutionDataflowBlockOptions {
            BoundedCapacity = downloadSetting.MultiPartsCount,
            MaxDegreeOfParallelism = downloadSetting.MultiPartsCount,
            CancellationToken = tokenSource.Token
        });

        var actionBlock = new ActionBlock<(HttpResponseMessage, MultiPartRange)>
            (async t => await WriteFileFromHttpResponseAsync(t.Item2.TempFilePath, t.Item1, tokenSource),
            new ExecutionDataflowBlockOptions {
                BoundedCapacity = downloadSetting.MultiPartsCount,
                MaxDegreeOfParallelism = downloadSetting.MultiPartsCount,
                CancellationToken = tokenSource.Token
            });

        var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };
        var transformManyBlock = new TransformManyBlock<IEnumerable<MultiPartRange>, MultiPartRange>(chunk => chunk,
            new ExecutionDataflowBlockOptions());

        transformManyBlock.LinkTo(transformBlock, linkOptions);
        transformBlock.LinkTo(actionBlock, linkOptions);

        transformManyBlock.Post(rangesList);
        transformManyBlock.Complete();

        await actionBlock.Completion;

        await using (var outputStream = File.Create(absolutePath)) {
            foreach (var inputFile in rangesList) {
                await using (var inputStream = File.OpenRead(inputFile.TempFilePath)) {
                    outputStream.Seek(inputFile.Start, SeekOrigin.Begin);
                    await inputStream.CopyToAsync(outputStream, tokenSource.Token);
                }

                File.Delete(inputFile.TempFilePath);
            }
        }

        return new FileInfo(absolutePath).Length;
    }
}