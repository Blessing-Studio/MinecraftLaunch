namespace MinecraftLaunch.Base.Enums;

public enum InstallStep {
    //Common
    Started,
    DownloadVersionJson,
    ParseMinecraft,
    DownloadLibraries,
    DownloadAssetIndexFile,
    RanToCompletion,

    //Forge Optifine
    DownloadPackage,
    ParsePackage,
    WriteVersionJsonAndSomeDependencies,
    RunInstallProcessor,

    //Modpack
    ParseDownloadUrls,
    DownloadMods,
    ExtractModpack,
    RedirectInvalidMod,

    //Error handle
    Interrupted = -1
}