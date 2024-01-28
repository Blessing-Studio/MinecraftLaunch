using MinecraftLaunch.Classes.Interfaces;

namespace MinecraftLaunch.Classes.Models.Download;

/// <summary>
/// 资源补全器返回信息
/// </summary>
public sealed record ResourceDownloadResponse {
    public required IEnumerable<IDownloadEntry> FailedResources { get; set; }
}