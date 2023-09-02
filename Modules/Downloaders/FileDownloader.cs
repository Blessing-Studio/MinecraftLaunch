using Flurl.Http;
using MinecraftLaunch.Modules.Models;
using MinecraftLaunch.Modules.Models.Download;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace MinecraftLaunch.Modules.Downloaders;

public class FileDownloader : DownloaderBase<FileDownloaderResponse, FileDownloaderProgressChangedEventArgs> {
    public FileDownloader(DownloadRequest downloadRequest, long? rangeFrom = null, long? rangeTo = null) {
        this.Url = downloadRequest.Url;

        this.DownloadFolder = downloadRequest.Directory.FullName;
        this.TargetFileName = downloadRequest.FileName;

        this.RangeFrom = rangeFrom;
        this.RangeTo = rangeTo;
    }

    public FileDownloader(string url, string downloadFolder, string targetFileName = null, long? rangeFrom = null, long? rangeTo = null) {
        this.Url = url;

        this.DownloadFolder = downloadFolder;
        this.TargetFileName = targetFileName;

        this.RangeFrom = rangeFrom;
        this.RangeTo = rangeTo;
    }

    public long TotleLength { get; private set; }

    public long CompletedLength { get; private set; }

    public string Url { get; private set; }

    public string TargetFileName { get; private set; }

    public string DownloadFolder { get; private set; }

    private long? RangeFrom;

    private long? RangeTo;

    public HttpResponseMessage HttpResponseMessage { get; private set; }

    public override void BeginDownload() {
        base.BeginDownload();

        DownloadProcess = Task.Run(async () => {
            if (RangeFrom.HasValue && RangeTo.HasValue)
                HttpResponseMessage = (await Url.WithHeader("Range", $"bytes={RangeFrom}-{RangeTo}")
                .GetAsync(HttpCompletionOption.ResponseHeadersRead)).ResponseMessage;

            else HttpResponseMessage = (await Url.GetAsync(HttpCompletionOption.ResponseHeadersRead)).ResponseMessage;

            TotleLength = (long)HttpResponseMessage.Content.Headers.ContentLength;

            if (HttpResponseMessage.Content.Headers.ContentDisposition != null && !string.IsNullOrEmpty(HttpResponseMessage.Content.Headers.ContentDisposition.FileName) && string.IsNullOrEmpty(TargetFileName))
                TargetFileName = HttpResponseMessage.Content.Headers.ContentDisposition.FileName.Trim('\"');

            if (string.IsNullOrEmpty(TargetFileName))
                TargetFileName = Path.GetFileName(HttpResponseMessage.RequestMessage.RequestUri.AbsoluteUri);

            HttpResponseMessage.EnsureSuccessStatusCode();

            if (!Directory.Exists(DownloadFolder))
                Directory.CreateDirectory(DownloadFolder);

            using var stream = await HttpResponseMessage.Content.ReadAsStreamAsync();
            using var fileStream = File.Create(Path.Combine(DownloadFolder, TargetFileName));

            byte[] buffer = new byte[ReadBufferSize];
            int readSize = 0;

            async Task<bool> Read() { readSize = await stream.ReadAsync(buffer, 0, buffer.Length); return readSize > 0; };

            while (await Read()) {
                await fileStream.WriteAsync(buffer, 0, readSize);
                CompletedLength += readSize;

                OnProgressChanged(new FileDownloaderProgressChangedEventArgs {
                    CompletedLength = CompletedLength,
                    TotleLength = TotleLength
                });
            }

            await fileStream.FlushAsync();
        }).ContinueWith(task => {
            DownloadTimeStopwatch.Stop();

            if (task.IsFaulted)
                OnDownloadFailed(task.Exception);

            var simpleDownloaderResponse = new FileDownloaderResponse {
                Exception = task.Exception,
                Success = !task.IsFaulted,
                CompletionType = task.IsFaulted ? DownloaderCompletionType.Uncompleted : DownloaderCompletionType.AllCompleted,
                DownloadTime = DownloadTimeStopwatch.Elapsed,
                Result = new FileInfo(Path.Combine(DownloadFolder, TargetFileName)),
                HttpStatusCode = HttpResponseMessage.StatusCode
            };

            OnDownloadCompleted(simpleDownloaderResponse);
            return simpleDownloaderResponse;
        });
    }

    public override void Dispose() {
        HttpResponseMessage.Dispose();
        DownloadProcess.Dispose();

        HttpResponseMessage = null;
        DownloadProcess = null;

        GC.Collect();
    }

    public static async ValueTask<FileDownloaderResponse> DownloadAsync(DownloadRequest downloadRequest) {
        using var downloader = new FileDownloader(downloadRequest);
        downloader.BeginDownload();

        return await downloader.CompleteAsync();
    }

    public static FileDownloader Build(DownloadRequest downloadRequest) {
        return new FileDownloader(downloadRequest);
    }
}
