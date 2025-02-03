using MinecraftLaunch.Base.Enums;
using System.Collections.Frozen;

namespace MinecraftLaunch.Base.Models.Network;

public record DownloadResult {
    public Exception Exception { get; init; }
    public DownloadResultType Type { get; init; }

    public DownloadResult(DownloadResultType type) {
        Type = type;
    }
}

public record GroupDownloadResult {
    public required DownloadResultType Type { get; init; }
    public required FrozenDictionary<DownloadRequest, DownloadResult> Failed { get; init; }
}