using MinecraftLaunch.Classes.Enums;

namespace MinecraftLaunch.Classes.Models.Game;

public sealed record GameLogEntry {
    public string Log { get; set; }

    public string Time { get; set; }

    public string Source { get; set; }

    public LogType LogType { get; set; }
}