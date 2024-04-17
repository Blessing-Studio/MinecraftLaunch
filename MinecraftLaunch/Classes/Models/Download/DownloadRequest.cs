namespace MinecraftLaunch.Classes.Models.Download;

/// <summary>
/// 下载请求信息配置记录类
/// </summary>
public sealed record DownloadRequest {
    /// <summary>
    /// 大文件判断阈值
    /// </summary>
    public long FileSizeThreshold { get; set; }

    /// <summary>
    /// 分片数量
    /// </summary>
    public int MultiPartsCount { get; set; }

    /// <summary>
    /// 最大并行下载线程数
    /// </summary>
    public int MultiThreadsCount { get; set; }

    /// <summary>
    /// 下载链接
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// 保存文件信息
    /// </summary>
    public FileInfo FileInfo { get; set; }

    /// <summary>
    /// 是否启用大文件分片下载
    /// </summary>
    public bool IsPartialContentSupported { get; set; }
}