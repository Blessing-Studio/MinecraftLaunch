namespace MinecraftLaunch.Classes.Models.Event;

public sealed class LogReceivedEventArgs(string log) : EventArgs {
    public string Text => log;
}