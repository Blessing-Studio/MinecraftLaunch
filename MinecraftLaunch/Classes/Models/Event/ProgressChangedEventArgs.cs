namespace MinecraftLaunch.Classes.Models.Event;

public class ProgressChangedEventArgs(TaskStatus status, double progress, string progressStatus) : EventArgs {
    public double Progress => progress;

    public TaskStatus Status => status;

    public string ProgressStatus => progressStatus;
}