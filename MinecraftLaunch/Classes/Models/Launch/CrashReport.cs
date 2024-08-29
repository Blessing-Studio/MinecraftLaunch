using MinecraftLaunch.Classes.Enums;

namespace MinecraftLaunch.Classes.Models.Launch;

public sealed record CrashReport {
    public string Original { get; set; }
    public CrashCauses CrashCauses { get; set; }
    public IReadOnlyCollection<string> Details { get; set; }
}