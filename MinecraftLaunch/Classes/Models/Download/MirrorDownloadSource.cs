namespace MinecraftLaunch.Classes.Models.Download;

/// <summary>
/// 镜像下载源
/// </summary>
public sealed record MirrorDownloadSource {
    public required string Host { get; set; }
    public required string VersionManifestUrl { get; set; }
    public required Dictionary<string, string> AssetsUrls { get; set; }
    public required Dictionary<string, string> LibrariesUrls { get; set; }
}