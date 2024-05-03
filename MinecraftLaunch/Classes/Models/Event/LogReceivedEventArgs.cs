using MinecraftLaunch.Classes.Enums;
using MinecraftLaunch.Classes.Models.Game;

namespace MinecraftLaunch.Classes.Models.Event;

public sealed class LogReceivedEventArgs(string original, string log, string time, string source, LogType logType) : EventArgs {
    public string Log => log;
    public string Time => time;
    public string Source => source;
    public string Original => original;

    public LogType LogType => logType;
}