namespace MinecraftLaunch.Classes.Models.Download;

public record MultiPartRange {
    public long End { get; set; }

    public long Start { get; set; }

    public string TempFilePath { get; set; }
}