namespace MinecraftLaunch.Classes.Enums;

public enum CrashCauses {

    #region Memory

    NoEnoughMemory,
    NoEnoughMemory32,

    #endregion Memory

    #region Java

    JdkUse,
    OpenJ9Use,
    JavaVersionTooHigh,
    UnsupportedJavaVersion,

    #endregion Java

    #region GPU

    UnsupportedNvDriver,
    UnsupportedAmdDriver,
    UnableToSetPixelFormat,
    UnsupportedIntelDriver,

    #endregion GPU

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

    #endregion Mod

    #region OpenGL

    OpenGl1282Error,
    GpuDoesNotSupportOpenGl,

    #endregion OpenGL

    #region Shaders

    TextureTooLargeOrLowEndGpu,
    FailedToLoadWorldBecauseOptiFine,

    #endregion Shaders

    #region AffiliatedComponent

    ForgeError,
    FabricError,
    FabricErrorWithSolution,
    MultipleForgeInVersionJson,
    IncompatibleForgeAndOptifine,
    LegacyForgeDoesNotSupportNewerJava,

    #endregion AffiliatedComponent

    LogFileNotFound,
    BlockCausedGameCrash,
    EntityCausedGameCrash,
    ContentValidationFailed,
    ManuallyTriggeredDebugCrash,
    IncorrectPathEncodingOrMainClassNotFound,

    Other
}