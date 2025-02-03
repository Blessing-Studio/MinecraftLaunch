using MinecraftLaunch.Base.Enums;

namespace MinecraftLaunch.Base.Models.Game;

public record struct ModLoaderInfo(ModLoaderType Type, string Version);