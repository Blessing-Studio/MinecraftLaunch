using MinecraftLaunch.Base.Enums;

namespace MinecraftLaunch.Base.Interfaces;

public interface IInstallEntry {
    string McVersion { get; }
    ModLoaderType ModLoaderType { get; }
}