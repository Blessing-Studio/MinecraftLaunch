namespace MinecraftLaunch.Events;

public class ProgressChangedEventArgs : EventArgs
{
    public string? ProgressDescription { get; set; }

    public double Progress { get; set; }
}