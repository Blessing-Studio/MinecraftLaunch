namespace MinecraftLaunch.Base.EventArgs;

public sealed class InstallProgressChangedEventArgs : System.EventArgs {
    public double Speed { get; set; }
    public double Progress { get; set; }
    public TaskStatus Status { get; set; }
    public string ProgressStatus { get; set; }
}