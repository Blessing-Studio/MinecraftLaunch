namespace MinecraftLaunch.Classes.Models.Event;

public sealed class ProgressChangedEventArgs(TaskStatus status, double progress, string progressStatus) : EventArgs {
    public double Progress => progress;

    public TaskStatus Status => status;

    public string ProgressStatus => progressStatus;
}