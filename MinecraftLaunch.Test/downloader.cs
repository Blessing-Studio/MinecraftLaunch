using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net;
using System.Threading.Tasks.Dataflow;
using System.Security.AccessControl;
using System.Security.Cryptography;
using MinecraftLaunch.Extensions;

public static class DownloadHelper {
    /// <summary>
    ///     下载线程
    /// </summary>
    public static int DownloadThread { get; set; } = 8;

    const int DefaultBufferSize = 1024 * 1024 * 4;
    static HttpClient Head => new();
    static HttpClient Data => new();
    static HttpClient MultiPart => new();

    #region 下载数据

    /// <summary>
    ///     下载文件（通过线程池）
    /// </summary>
    /// <param name="downloadFile"></param>
    /// <param name="downloadSettings"></param>
    /// <returns></returns>
    public static async Task DownloadData(DownloadFile downloadFile, DownloadSettings? downloadSettings = null) {
        downloadSettings ??= DownloadSettings.Default;

        var filePath = Path.Combine(downloadFile.DownloadPath, downloadFile.FileName);
        var exceptions = new List<Exception>();

        for (var i = 0; i <= downloadSettings.RetryCount; i++) {
            using var cts = new CancellationTokenSource(downloadSettings.Timeout * Math.Max(1, i + 1));

            try {
                using var request = new HttpRequestMessage(HttpMethod.Get, downloadFile.DownloadUri);

                if (downloadSettings.Authentication != null)
                    request.Headers.Authorization = downloadSettings.Authentication;
                if (!string.IsNullOrEmpty(downloadSettings.Host))
                    request.Headers.Host = downloadSettings.Host;

                using var res =
                    await Data.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cts.Token);

                res.EnsureSuccessStatusCode();

                await using var stream = await res.Content.ReadAsStreamAsync(cts.Token);
                await using var outputStream = File.Create(filePath);

                var responseLength = res.Content.Headers.ContentLength ?? 0;
                var downloadedBytesCount = 0L;
                var sw = new Stopwatch();

                var tSpeed = 0d;
                var cSpeed = 0;

                using var rentMemory = Pool.Rent(DefaultBufferSize);

                while (true) {
                    sw.Restart();
                    var bytesRead = await stream.ReadAsync(rentMemory.Memory, cts.Token);
                    sw.Stop();

                    if (bytesRead == 0) break;

                    await outputStream.WriteAsync(rentMemory.Memory[..bytesRead], cts.Token);

                    downloadedBytesCount += bytesRead;

                    var elapsedTime = sw.Elapsed.TotalSeconds == 0 ? 1 : sw.Elapsed.TotalSeconds;
                    var speed = bytesRead / elapsedTime;

                    tSpeed += speed;
                    cSpeed++;

                    downloadFile.OnChanged(
                        speed,
                        (double)downloadedBytesCount / responseLength,
                        downloadedBytesCount,
                        responseLength);
                }

                sw.Stop();

                if (downloadSettings.CheckFile && !string.IsNullOrEmpty(downloadFile.CheckSum)) {
                    await outputStream.FlushAsync(cts.Token);
                    outputStream.Seek(0, SeekOrigin.Begin);

                    var checkSum = (await downloadSettings.HashDataAsync(outputStream, cts.Token)).BytesToString();

                    if (!(checkSum?.Equals(downloadFile.CheckSum, StringComparison.OrdinalIgnoreCase) ?? false)) {
                        downloadFile.RetryCount++;
                        continue;
                    }
                }

                var aSpeed = tSpeed / cSpeed;
                downloadFile.OnCompleted(true, null, aSpeed);

                return;
            }
            catch (Exception e) {
                await Task.Delay(250, cts.Token);

                downloadFile.RetryCount++;
                exceptions.Add(e);
            }
        }

        downloadFile.OnCompleted(false, new AggregateException(exceptions), 0);
    }

    #endregion

    public static bool SetMaxThreads() {
        ThreadPool.GetMaxThreads(out var maxWorkerThreads,
            out var maxConcurrentActiveRequests);

        var changeSucceeded = ThreadPool.SetMaxThreads(
            maxWorkerThreads, maxConcurrentActiveRequests);

        return changeSucceeded;
    }

    public static string AutoFormatSpeedString(double speedInBytePerSecond) {
        var speed = AutoFormatSpeed(speedInBytePerSecond);
        var unit = speed.Unit switch {
            SizeUnit.B => "B / s",
            SizeUnit.Kb => "Kb / s",
            SizeUnit.Mb => "Mb / s",
            SizeUnit.Gb => "Gb / s",
            SizeUnit.Tb => "Tb / s",
            _ => "B / s"
        };

        return $"{speed.Speed:F} {unit}";
    }

    public static (double Speed, SizeUnit Unit) AutoFormatSpeed(double transferSpeed) {
        const double baseNum = 1024;

        // Auto choose the unit
        var unit = SizeUnit.B;

        if (transferSpeed > baseNum) {
            unit = SizeUnit.Kb;
            if (transferSpeed > Math.Pow(baseNum, 2)) {
                unit = SizeUnit.Mb;
                if (transferSpeed > Math.Pow(baseNum, 3)) {
                    unit = SizeUnit.Gb;
                    if (transferSpeed > Math.Pow(baseNum, 4)) {
                        unit = SizeUnit.Tb;
                    }
                }
            }
        }

        var convertedSpeed = unit switch {
            SizeUnit.Kb => transferSpeed / baseNum,
            SizeUnit.Mb => transferSpeed / Math.Pow(baseNum, 2),
            SizeUnit.Gb => transferSpeed / Math.Pow(baseNum, 3),
            SizeUnit.Tb => transferSpeed / Math.Pow(baseNum, 4),
            _ => transferSpeed
        };

        return (convertedSpeed, unit);
    }

    #region 下载一个列表中的文件（自动确定是否使用分片下载）

    /// <summary>
    ///     下载文件方法（自动确定是否使用分片下载）
    /// </summary>
    /// <param name="df"></param>
    /// <param name="downloadSettings"></param>
    public static Task AdvancedDownloadFile(DownloadFile df, DownloadSettings downloadSettings) {
        if (!Directory.Exists(df.DownloadPath))
            Directory.CreateDirectory(df.DownloadPath);

        if (df.FileSize is >= 1048576 or 0)
            return MultiPartDownloadTaskAsync(df, downloadSettings);

        return DownloadData(df, downloadSettings);
    }

    /// <summary>
    ///     下载文件方法（自动确定是否使用分片下载）
    /// </summary>
    /// <param name="fileEnumerable">文件列表</param>
    /// <param name="downloadSettings"></param>
    public static async Task AdvancedDownloadListFile(
        IEnumerable<DownloadFile> fileEnumerable,
        DownloadSettings downloadSettings) {
        SetMaxThreads();

        var actionBlock = new ActionBlock<DownloadFile>(
            d => AdvancedDownloadFile(d, downloadSettings),
            new ExecutionDataflowBlockOptions {
                BoundedCapacity = DownloadThread * 2,
                MaxDegreeOfParallelism = DownloadThread
            });

        foreach (var downloadFile in fileEnumerable) {
            await actionBlock.SendAsync(downloadFile);
        }

        actionBlock.Complete();
        await actionBlock.Completion;
    }

    #endregion

    #region 分片下载

    static readonly MemoryPool<byte> Pool = MemoryPool<byte>.Shared;

    /// <summary>
    ///     分片下载方法（异步）
    /// </summary>
    /// <param name="downloadFile"></param>
    /// <param name="downloadSettings"></param>
    /// <returns></returns>
    public static async Task MultiPartDownloadTaskAsync(
        DownloadFile? downloadFile,
        DownloadSettings? downloadSettings = null) {
        if (downloadFile == null) return;

        downloadSettings ??= DownloadSettings.Default;

        if (downloadSettings.DownloadParts <= 0)
            downloadSettings.DownloadParts = Environment.ProcessorCount;

        var exceptions = new List<Exception>();
        var filePath = Path.Combine(downloadFile.DownloadPath, downloadFile.FileName);
        var timeout = TimeSpan.FromMilliseconds(downloadSettings.Timeout * 2);

        var isLatestFileCheckSucceeded = true;
        List<DownloadRange>? readRanges = null;

        for (var r = 0; r <= downloadSettings.RetryCount; r++) {
            using var cts = new CancellationTokenSource(timeout * Math.Max(1, r + 1));

            try {
                #region Get file size

                using var headReq = new HttpRequestMessage(HttpMethod.Head, downloadFile.DownloadUri);

                if (downloadSettings.Authentication != null)
                    headReq.Headers.Authorization = downloadSettings.Authentication;
                if (!string.IsNullOrEmpty(downloadSettings.Host))
                    headReq.Headers.Host = downloadSettings.Host;

                using var headRes = await Head.SendAsync(headReq, cts.Token);

                headRes.EnsureSuccessStatusCode();

                var responseLength = headRes.Content.Headers.ContentLength ?? 0;
                var hasAcceptRanges = headRes.Headers.AcceptRanges.Count != 0;

                using var rangeGetMessage = new HttpRequestMessage(HttpMethod.Get, downloadFile.DownloadUri);
                rangeGetMessage.Headers.Range = new RangeHeaderValue(0, 0);

                if (downloadSettings.Authentication != null)
                    rangeGetMessage.Headers.Authorization = downloadSettings.Authentication;
                if (!string.IsNullOrEmpty(downloadSettings.Host))
                    rangeGetMessage.Headers.Host = downloadSettings.Host;

                using var rangeGetRes = await Head.SendAsync(rangeGetMessage, cts.Token);

                var parallelDownloadSupported =
                    responseLength != 0 &&
                    hasAcceptRanges &&
                    rangeGetRes.StatusCode == HttpStatusCode.PartialContent &&
                    (rangeGetRes.Content.Headers.ContentRange?.HasRange ?? false) &&
                    rangeGetRes.Content.Headers.ContentLength == 1;

                if (!parallelDownloadSupported) {
                    await DownloadData(downloadFile, downloadSettings);
                    return;
                }

                #endregion

                if (!Directory.Exists(downloadFile.DownloadPath))
                    Directory.CreateDirectory(downloadFile.DownloadPath);

                #region Calculate ranges

                readRanges = [];
                var partSize = responseLength / downloadSettings.DownloadParts;
                var totalSize = responseLength;

                while (totalSize > 0) {
                    //计算分片
                    var to = totalSize;
                    var from = totalSize - partSize;

                    if (from < 0) from = 0;

                    totalSize -= partSize;

                    readRanges.Add(new DownloadRange {
                        Start = from,
                        End = to,
                        TempFileName = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName())
                    });
                }

                #endregion

                #region Parallel download

                var downloadedBytesCount = 0L;
                var tasksDone = 0;
                var doneRanges = new ConcurrentBag<DownloadRange>();

                var streamBlock =
                    new TransformBlock<DownloadRange, (HttpResponseMessage, DownloadRange)>(
                        async p => {
                            using var request = new HttpRequestMessage(HttpMethod.Get, downloadFile.DownloadUri);

                            if (downloadSettings.Authentication != null)
                                request.Headers.Authorization = downloadSettings.Authentication;
                            if (!string.IsNullOrEmpty(downloadSettings.Host))
                                request.Headers.Host = downloadSettings.Host;

                            request.Headers.Range = new RangeHeaderValue(p.Start, p.End);

                            var downloadTask = await MultiPart.SendAsync(
                                request,
                                HttpCompletionOption.ResponseHeadersRead,
                                cts.Token);

                            return (downloadTask, p);
                        }, new ExecutionDataflowBlockOptions {
                            BoundedCapacity = downloadSettings.DownloadParts,
                            MaxDegreeOfParallelism = downloadSettings.DownloadParts
                        });

                var tSpeed = 0D;
                var cSpeed = 0;

                var writeActionBlock = new ActionBlock<(HttpResponseMessage, DownloadRange)>(async t => {
                    using var res = t.Item1;

                    await using var stream = await res.Content.ReadAsStreamAsync(cts.Token);
                    await using var fileToWriteTo = File.Create(t.Item2.TempFileName);
                    using var rentMemory = Pool.Rent(DefaultBufferSize);

                    var sw = new Stopwatch();

                    while (true) {
                        sw.Restart();
                        var bytesRead = await stream.ReadAsync(rentMemory.Memory, cts.Token);
                        sw.Stop();

                        if (bytesRead == 0)
                            break;

                        await fileToWriteTo.WriteAsync(rentMemory.Memory[..bytesRead], cts.Token);

                        Interlocked.Add(ref downloadedBytesCount, bytesRead);

                        var elapsedTime = Math.Ceiling(sw.Elapsed.TotalSeconds);
                        var speed = bytesRead / elapsedTime;

                        tSpeed += speed;
                        cSpeed++;

                        downloadFile.OnChanged(
                            speed,
                            (double)downloadedBytesCount / responseLength,
                            downloadedBytesCount,
                            responseLength);
                    }

                    sw.Stop();

                    Interlocked.Add(ref tasksDone, 1);
                    doneRanges.Add(t.Item2);
                }, new ExecutionDataflowBlockOptions {
                    BoundedCapacity = downloadSettings.DownloadParts,
                    MaxDegreeOfParallelism = downloadSettings.DownloadParts,
                    CancellationToken = cts.Token
                });

                var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };

                var filesBlock =
                    new TransformManyBlock<IEnumerable<DownloadRange>, DownloadRange>(chunk => chunk,
                        new ExecutionDataflowBlockOptions());

                filesBlock.LinkTo(streamBlock, linkOptions);
                streamBlock.LinkTo(writeActionBlock, linkOptions);
                filesBlock.Post(readRanges);

                filesBlock.Complete();

                await writeActionBlock.Completion;

                var aSpeed = tSpeed / cSpeed;

                if (doneRanges.Count != readRanges.Count) {
                    downloadFile.RetryCount++;
                    streamBlock.Complete();
                    writeActionBlock.Complete();
                    continue;
                }

                await using (var outputStream = File.Create(filePath)) {
                    foreach (var inputFilePath in readRanges) {
                        await using var inputStream = File.OpenRead(inputFilePath.TempFileName);
                        outputStream.Seek(inputFilePath.Start, SeekOrigin.Begin);

                        await inputStream.CopyToAsync(outputStream, cts.Token);
                    }

                    await outputStream.FlushAsync(cts.Token);
                    outputStream.Seek(0, SeekOrigin.Begin);

                    if (downloadSettings.CheckFile && !string.IsNullOrEmpty(downloadFile.CheckSum)) {
                        var checkSum = (await downloadSettings.HashDataAsync(outputStream, cts.Token)).BytesToString();

                        if (!checkSum.Equals(downloadFile.CheckSum, StringComparison.OrdinalIgnoreCase)) {
                            downloadFile.RetryCount++;
                            isLatestFileCheckSucceeded = false;
                            continue;
                        }

                        isLatestFileCheckSucceeded = true;
                    }
                }

                streamBlock.Complete();
                writeActionBlock.Complete();

                #endregion

                downloadFile.OnCompleted(true, null, aSpeed);
                return;
            }
            catch (Exception ex) {
                if (readRanges != null)
                    foreach (var piece in readRanges.Where(piece => File.Exists(piece.TempFileName)))
                        try {
                            File.Delete(piece.TempFileName);
                        }
                        catch (Exception e) {
                            Debug.WriteLine(e);
                        }

                downloadFile.RetryCount++;
                exceptions.Add(ex);
                // downloadFile.OnCompleted(false, ex, 0);
            }
        }

        if (exceptions.Count > 0) {
            downloadFile.OnCompleted(false, new AggregateException(exceptions), 0);
            return;
        }

        if (!isLatestFileCheckSucceeded) {
            downloadFile.OnCompleted(false, null, 0);
            return;
        }

        downloadFile.OnCompleted(true, null, 0);
    }

    #endregion
}

public enum HashType {
    MD5,
    SHA1,
    SHA256,
    SHA384,
    SHA512
}

public class DownloadSettings {
    public static DownloadSettings Default => new() {
        RetryCount = 0,
        CheckFile = false,
        Timeout = (int)TimeSpan.FromMinutes(5).TotalMilliseconds,
        DownloadParts = 16
    };

    public int RetryCount { get; init; }
    public bool CheckFile { get; init; }
    public int Timeout { get; init; }
    public int DownloadParts { get; set; }
    public HashType HashType { get; init; }

    /// <summary>
    /// 认证
    /// </summary>
    public AuthenticationHeaderValue? Authentication { get; init; }

    /// <summary>
    /// 请求源
    /// </summary>
    public string? Host { get; init; }

    public async ValueTask<byte[]> HashDataAsync(Stream stream, CancellationToken? token) {
        token ??= CancellationToken.None;

        return HashType switch {
            HashType.MD5 => await MD5.HashDataAsync(stream, token.Value),
            HashType.SHA1 => await SHA1.HashDataAsync(stream, token.Value),
            HashType.SHA256 => await SHA256.HashDataAsync(stream, token.Value),
            HashType.SHA384 => await SHA384.HashDataAsync(stream, token.Value),
            HashType.SHA512 => await SHA512.HashDataAsync(stream, token.Value),
            _ => throw new NotSupportedException()
        };
    }
}

public class DownloadFile {
    /// <summary>
    ///     下载Uri
    /// </summary>
    public required string DownloadUri { get; init; }

    /// <summary>
    ///     下载路径
    /// </summary>
    public required string DownloadPath { get; init; }

    /// <summary>
    ///     保存的文件名
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    ///     最大重试计数
    /// </summary>
    public int RetryCount { get; internal set; }

    /// <summary>
    ///     文件类型（仅在Lib/Asset补全时可用）
    /// </summary>
    public ResourceType FileType { get; init; }

    /// <summary>
    ///     文件大小
    /// </summary>
    public long FileSize { get; init; }

    /// <summary>
    ///     文件检验码
    /// </summary>
    public string? CheckSum { get; init; }

    /// <summary>
    ///     下载完成事件
    /// </summary>
    public event EventHandler<DownloadFileCompletedEventArgs>? Completed;

    /// <summary>
    ///     下载改变事件
    /// </summary>
    public event EventHandler<DownloadFileChangedEventArgs>? Changed;

    public void OnChanged(double speed, double progress, long bytesReceived, long totalBytes) {
        Changed?.Invoke(this, new DownloadFileChangedEventArgs {
            Speed = speed,
            ProgressPercentage = progress,
            BytesReceived = bytesReceived,
            TotalBytes = totalBytes
        });
    }

    public void OnCompleted(bool? success, Exception? ex, double averageSpeed) {
        Completed?.Invoke(this, new DownloadFileCompletedEventArgs(success, ex, averageSpeed));
    }
}

public enum SizeUnit {
    B,
    Kb,
    Mb,
    Gb,
    Tb
}

[DebuggerDisplay("[{Start}-{End}]")]
public readonly struct DownloadRange {
    /// <summary>
    ///     开始字节
    /// </summary>
    public required long Start { get; init; }

    /// <summary>
    ///     结束字节
    /// </summary>
    public required long End { get; init; }

    /// <summary>
    ///     临时文件名称
    /// </summary>
    public required string TempFileName { get; init; }
}

public class DownloadFileCompletedEventArgs(bool? success, Exception? ex, double averageSpeed) : EventArgs {
    public double AverageSpeed { get; set; } = averageSpeed;
    public bool? Success { get; } = success;
    public Exception? Error { get; } = ex;
}

public class DownloadFileChangedEventArgs : EventArgs {
    /// <summary>
    /// 速度：字节 /秒
    /// </summary>
    public double Speed { get; set; }
    public double ProgressPercentage { get; set; }
    public long BytesReceived { get; set; }
    public long? TotalBytes { get; set; }
}