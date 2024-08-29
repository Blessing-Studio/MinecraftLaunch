using MinecraftLaunch.Classes.Models.ServerPing;

namespace MinecraftLaunch.Classes.Models.Event;

public sealed class ServerLatencyChangedEventArgs : EventArgs {
    public long Latency { get; set; }
    public PingPayload Response { get; set; }
}