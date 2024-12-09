namespace MinecraftLaunch.Classes.Models.Download;

public sealed record DownloaderConfiguration {
    public readonly static DownloaderConfiguration Default = new DownloaderConfiguration {
        MaxThread = 128,
        IsEnableFragmentedDownload = true,
    };

    public int MaxThread { get; set; }
    public bool IsEnableFragmentedDownload { get; set; }
    public MirrorDownloadSource DownloadSource { get; set; }
}