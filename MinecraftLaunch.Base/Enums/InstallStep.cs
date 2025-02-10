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

    //Composite
    ParseInstaller,
    InstallVanilla,
    InstallPrimaryModLoader,
    InstallSecondaryModLoader,

    //Error handle
    Interrupted = -1,

    //undefined
    Undefined = -2
}