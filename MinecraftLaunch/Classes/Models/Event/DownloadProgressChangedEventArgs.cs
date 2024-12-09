namespace MinecraftLaunch.Classes.Models.Event;

public sealed class DownloadProgressChangedEventArgs : EventArgs {
    public double Speed { get; set; }
    public int TotalCount { get; set; }
    public int CompletedCount { get; set; }
}