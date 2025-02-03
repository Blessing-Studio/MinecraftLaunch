using System.Collections.Frozen;

namespace MinecraftLaunch.Base.Models.Network;

public record struct DownloadRequest {
    public long Size { get; set; }
    public string Url { get; set; }
    public FileInfo FileInfo { get; set; }
    public Action<long> BytesDownloaded { get; set; }

    public int MultiPartsCount { get; set; }
    public int MultiThreadsCount { get; set; }
    public long FileSizeThreshold { get; set; }
    public bool IsPartialContentSupported { get; set; }

    public DownloadRequest(string url, string localPath) {
        Url = url;
        FileInfo = new(localPath);
    }
}

public record GroupDownloadRequest {
    public DateTime StartTime { get; init; }

    public IEnumerable<DownloadRequest> Files { get; set; }
    public Action<double> DownloadSpeedChanged { get; set; }
    public Action<DownloadRequest, DownloadResult> SingleRequestCompleted { get; set; }

    public GroupDownloadRequest(IEnumerable<DownloadRequest> files) {
        Files = files;
        StartTime = DateTime.Now;
    }
}