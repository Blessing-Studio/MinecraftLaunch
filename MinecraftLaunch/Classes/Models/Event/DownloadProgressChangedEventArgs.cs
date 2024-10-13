namespace MinecraftLaunch.Classes.Models.Event;

public sealed class DownloadProgressChangedEventArgs : EventArgs {
    public int TotalCount { get; set; }

    public int CompletedCount { get; set; }

    public int FailedCount { get; set; }

    public int TotalBytes { get; set; }

    public int DownloadedBytes { get; set; }

    public double Speed { get; set; }
}