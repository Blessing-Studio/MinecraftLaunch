namespace MinecraftLaunch.Classes.Models.Download;

/// <summary>
/// 下载请求信息配置记录类
/// </summary>
public record struct DownloadRequest {
    /// <summary>
    /// 下载链接
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// 文件大小
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// 保存文件信息
    /// </summary>
    public FileInfo FileInfo { get; set; }

    /// <summary>
    /// 下载进度接收事件（单位：Byte）
    /// </summary>
    public Action<long> BytesDownloaded { get; set; }

    public int MultiPartsCount { get; set; }
    public int MultiThreadsCount { get; set; }
    public long FileSizeThreshold { get; set; }
    public bool IsPartialContentSupported { get; set; }
}

public class GroupDownloadRequest {
    public DateTime StartTime { get; init; }

    public IEnumerable<DownloadRequest> Files { get; set; }
    public Action<double> DownloadSpeedChanged { get; set; }
    public Action<DownloadRequest, DownloadResult> SingleRequestCompleted { get; set; }

    public GroupDownloadRequest(IEnumerable<DownloadRequest> files) {
        Files = files;
        StartTime = DateTime.Now;
    }
}