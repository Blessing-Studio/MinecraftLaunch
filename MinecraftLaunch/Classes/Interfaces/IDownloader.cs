using MinecraftLaunch.Classes.Models.Download;

namespace MinecraftLaunch.Classes.Interfaces;

/// <summary>
/// 下载器统一接口
/// </summary>
public interface IDownloader {
    /// <summary>
    /// 单个文件下载
    /// </summary>
    Task<DownloadResult> DownloadFileAsync(DownloadRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// 多个文件下载
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<GroupDownloadResult> DownloadFilesAsync(GroupDownloadRequest request, CancellationToken cancellationToken = default);
}