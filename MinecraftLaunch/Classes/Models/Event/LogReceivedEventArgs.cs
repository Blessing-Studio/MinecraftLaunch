using MinecraftLaunch.Classes.Enums;
using MinecraftLaunch.Classes.Models.Game;

namespace MinecraftLaunch.Classes.Models.Event;

public sealed class LogReceivedEventArgs(string log, string time, string source, LogType logType) : EventArgs {
    public string Text => log;
    public string Time => time;
    public string Source => source;
    public LogType LogType => logType;
}