using MinecraftLaunch.Classes.Enums;
using System.Collections.Frozen;

namespace MinecraftLaunch.Classes.Models.Download;

public record DownloadResult {
    public DownloadResultType Type { get; init; }

    public Exception? Exception { get; init; }

    public DownloadResult(DownloadResultType type) {
        Type = type;
    }
}

public record GroupDownloadResult {
    public required DownloadResultType Type { get; init; }
    public required FrozenDictionary<DownloadRequest, DownloadResult> Failed { get; init; }
}