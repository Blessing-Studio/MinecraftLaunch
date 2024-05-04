namespace MinecraftLaunch.Classes.Enums;

public enum CrashCauses {
    #region Memory

    NoEnoughMemory,
    NoEnoughMemory32,

    #endregion

    #region Java

    JdkUse,
    OpenJ9Use,
    JavaVersionTooHigh,
    UnsupportedJavaVersion,

    #endregion

    #region GPU

    UnsupportedNvDriver,
    UnsupportedAmdDriver,
    UnableToSetPixelFormat,
    UnsupportedIntelDriver,

    #endregion

    #region Mod

    DuplicateMod,
    ModIdExceeded,
    ModInitFailed,
    ModMixinFailed,
    ModLoaderError,
    DecompressedMod,
    IncorrectModConfig,
    ModCausedGameCrash,
    MissingOrUnsupportedMandatoryMod,

    #endregion

    #region OpenGL

    OpenGl1282Error,
    GpuDoesNotSupportOpenGl,

    #endregion

    #region Shaders

    TextureTooLargeOrLowEndGpu,
    FailedToLoadWorldBecauseOptiFine, 

    #endregion

    #region AffiliatedComponent

    ForgeError,
    FabricError,
    FabricErrorWithSolution,
    MultipleForgeInVersionJson,
    IncompatibleForgeAndOptifine,
    LegacyForgeDoesNotSupportNewerJava,

    #endregion

    LogFileNotFound,
    BlockCausedGameCrash,
    EntityCausedGameCrash,
    ContentValidationFailed,
    ManuallyTriggeredDebugCrash,
    IncorrectPathEncodingOrMainClassNotFound,

    Other
}
