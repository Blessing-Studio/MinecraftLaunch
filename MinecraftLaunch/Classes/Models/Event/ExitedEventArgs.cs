namespace MinecraftLaunch.Classes.Models.Event;

public sealed class ExitedEventArgs(int exitCode) : EventArgs {
    public int ExitCode => exitCode;
}