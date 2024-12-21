namespace MinecraftLaunch.Classes.Models.Download;

public sealed record DownloaderConfiguration {
    public readonly static DownloaderConfiguration Default = new() {
        MaxThread = 128,
        IsEnableFragmentedDownload = true,
    };

    public int MaxThread { get; set; }
    public bool IsEnableFragmentedDownload { get; set; }
}