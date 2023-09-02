using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftLaunch.Modules.Models.Download {
    public class DownloadRequest {
        public DirectoryInfo Directory { get; set; }

        public string Url { get; set; }

        public string FileName { get; set; }

        public long? FileSize { get; set; }
    }

    public enum DownloaderCompletionType {
        AllCompleted = 0,
        PartiallyCompleted = 1,
        Uncompleted = 2
    }

    public interface IDownloaderProgressChangedEventArgs {
        public double Progress { get; }
    }

    public class DownloadResponse {
        public HttpStatusCode HttpStatusCode { get; set; }

        public DownloaderCompletionType CompletionType { get; set; }

        public TimeSpan DownloadTime { get; set; }

        public Exception Exception { get; set; }
    }

    public class DownloaderResponse<TResult> : DownloadResponse {
        public TResult Result { get; set; }
    }

    public class FileDownloaderProgressChangedEventArgs : IDownloaderProgressChangedEventArgs {
        public double Progress => CompletedLength / (double)TotleLength;

        public long TotleLength { get; set; }

        public long CompletedLength { get; set; }
    }

    public class FileDownloaderResponse : DownloaderResponse<FileInfo> {
        public bool Success { get; set; }
    }
}
