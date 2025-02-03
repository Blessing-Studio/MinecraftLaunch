namespace MinecraftLaunch.Base.EventArgs;

public sealed class ResourceDownloadProgressChangedEventArgs : System.EventArgs {
    public double Speed { get; set; }
    public int TotalCount { get; set; }
    public int CompletedCount { get; set; }
}