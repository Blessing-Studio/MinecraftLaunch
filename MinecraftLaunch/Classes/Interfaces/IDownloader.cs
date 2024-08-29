namespace MinecraftLaunch.Classes.Interfaces;

/// <summary>
/// 下载器统一接口
/// </summary>
public interface IDownloader {

    /// <summary>
    /// 异步下载方法
    /// </summary>
    ValueTask<bool> DownloadAsync();
}