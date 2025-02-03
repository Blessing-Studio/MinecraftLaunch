using MinecraftLaunch.Base.Models.Game;
using NbtToolkit.Binary;

namespace MinecraftLaunch.Base.Interfaces;

public interface INbtParser {
    NbtReader GetReader();
    NbtWriter GetWriter();
    Task<SaveEntry> ParseSaveAsync(string saveName, bool @bool = true, CancellationToken cancellationToken = default);
}