using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftLaunch.Modules.Enum {
    public enum CrashReason {
        // Game
        FileOrContentCheckFailed,
        SpecificBlockCausedCrash,
        SpecificEntityCausedCrash,
        TextureTooLargeOrInsufficientGraphicsConfig,
        ShaderOrResourcePackCausedOpenGL1282Error,
        UnableToLoadTexture,

        // Mod
        ModConfigCausedGameCrash,
        ModMixinFailed,
        ModLoaderError,
        ModInitializationFailed,
        ModFileDecompressed,
        TooManyModsExceededIDLimit,
        ModCausedGameCrash,
        ModInstalledRepeatedly,

        // ModLoader
        OptiFineIncompatibleWithForge,
        FabricError,
        FabricErrorWithSolution,
        ForgeError,
        LowVersionForgeIncompatibleWithHighVersionJava,
        MultipleForgeInVersionJson,
        OptiFineCausedWorldLoadingFailure,

        // Log
        CrashLogStackAnalysisFoundKeyword,
        CrashLogStackAnalysisFoundModName,
        MCLogStackAnalysisFoundKeyword,

        // Jvm
        InsufficientMemory,
        UsingJDK,
        GraphicsCardDoesNotSupportOpenGL,
        UsingOpenJ9,
        JavaVersionTooHigh,
        UnsupportedJavaClassVersionError,
        Using32BitJavaCausedInsufficientJVMMemory,

        // Player
        PlayerTriggeredDebugCrash
    }
}
