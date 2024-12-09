namespace MinecraftLaunch.Classes.Models.Event;

public sealed class ProgressChangedEventArgs(TaskStatus status, double progress, string progressStatus, double speed = default) : EventArgs {
    public double Speed => speed;
    public double Progress => progress;
    public TaskStatus Status => status;
    public string ProgressStatus => progressStatus;
}