using System.Diagnostics;
using MinecraftLaunch.Classes.Models.Event;

namespace MinecraftLaunch.Classes.Interfaces;

public class IGameProcessWatcher {
    public Process Process { get; }

    public IEnumerable<string> Arguments { get; }

    public event EventHandler<ExitedEventArgs> Exited;

    public event EventHandler<LogReceivedEventArgs> OutputLogReceived;
}