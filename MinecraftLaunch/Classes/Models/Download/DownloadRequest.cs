namespace MinecraftLaunch.Classes.Models.Download;

/// <summary>
/// 下载请求信息配置记录类
/// </summary>
public sealed record DownloadRequest {
    public int Size { get; set; }

    public bool IsCompleted { get; set; }

    public int DownloadedBytes { get; set; }

    public required string Url { get; init; }

    public required FileInfo FileInfo { get; set; }

    public bool IsPartialContentSupported { get; set; }
}