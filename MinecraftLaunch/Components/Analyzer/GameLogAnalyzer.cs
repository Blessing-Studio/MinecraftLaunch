using MinecraftLaunch.Extensions;
using MinecraftLaunch.Classes.Enums;
using MinecraftLaunch.Classes.Models.Game;
using MinecraftLaunch.Classes.Models.Launch;

using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace MinecraftLaunch.Components.Analyzer;

// Reference: https://github.com/Hex-Dragon/PCL2
// Reference: https://github.com/huanghongxun/HMCL
// Reference: https://github.com/Corona-Studio/ProjBobcat

/// <summary>
/// 游戏崩溃分析器
/// </summary>
public sealed partial class GameCrashAnalyzer(GameEntry gameEntry, bool isIndependencyCore) {
    private IEnumerable<string> _gameLogs;
    private List<string> _crashLogs = new();

    private readonly GameEntry _gameEntry = gameEntry;
    private readonly bool _isIndependencyCore = isIndependencyCore;
    private readonly Dictionary<string, CrashCauses> _logCrashCauses = new() {
        { ".J9VMInternals.", CrashCauses.OpenJ9Use },
        { "OpenJ9 is incompatible", CrashCauses.OpenJ9Use },
        { "Out of Memory Error", CrashCauses.NoEnoughMemory },
        { "Open J9 is not supported", CrashCauses.OpenJ9Use },
        { "1282: Invalid operation", CrashCauses.OpenGl1282Error },
        { "maximum id range exceeded", CrashCauses.ModIdExceeded },
        { "Caught exception from ", CrashCauses.ModCausedGameCrash },
        { "java.lang.OutOfMemoryError", CrashCauses.NoEnoughMemory },
        { "java.lang.ClassCastException: class jdk.", CrashCauses.JdkUse },
        { "Couldn't set pixel format", CrashCauses.UnableToSetPixelFormat },
        { "java.lang.ClassCastException: java.base/jdk", CrashCauses.JdkUse },
        { "Pixel format not accelerated", CrashCauses.UnableToSetPixelFormat },
        { "java.lang.NoSuchFieldException: ucp", CrashCauses.JavaVersionTooHigh },
        { "Manually triggered debug crash", CrashCauses.ManuallyTriggeredDebugCrash },
        { "Unsupported class file major version", CrashCauses.UnsupportedJavaVersion },
        { "because module java.base does not export", CrashCauses.JavaVersionTooHigh },
        { "The system is out of physical RAM or swap space", CrashCauses.NoEnoughMemory },
        { "Extracted mod jars found, loading will NOT continue", CrashCauses.DecompressedMod },
        { "The driver does not appear to support OpenGL", CrashCauses.GpuDoesNotSupportOpenGl },
        { "Maybe try a lower resolution resourcepack?", CrashCauses.TextureTooLargeOrLowEndGpu },
        { "java.lang.ClassNotFoundException: java.lang.invoke.LambdaMetafactory", CrashCauses.JavaVersionTooHigh },
        {
            "TRANSFORMER/net.optifine/net.optifine.reflect.Reflector.<clinit>(Reflector.java",
            CrashCauses.IncompatibleForgeAndOptifine
        },
        {
            "Found multiple arguments for option fml.forgeVersion, but you asked for only one",
            CrashCauses.MultipleForgeInVersionJson
        },
        {
            "java.lang.ClassNotFoundException: jdk.nashorn.api.scripting.NashornScriptEngineFactory",
            CrashCauses.JavaVersionTooHigh
        },
        {
            "The directories below appear to be extracted jar files. Fix this before you continue.",
            CrashCauses.DecompressedMod
        },
        {
            "java.lang.NoSuchMethodError: sun.security.util.ManifestEntryVerifier",
            CrashCauses.LegacyForgeDoesNotSupportNewerJava
        },
        {
            "java.lang.UnsupportedClassVersionError: net/fabricmc/loader/impl/launch/knot/KnotClient : Unsupported major.minor version",
            CrashCauses.UnsupportedJavaVersion
        },
        {
            "java.lang.NoSuchMethodError: 'void net.minecraft.client.renderer.block.model.BakedQuad.<init>(int[], int, net.minecraft.core.Direction, net.minecraft.client.renderer.texture.TextureAtlasSprite, boolean, boolean)'",
            CrashCauses.IncompatibleForgeAndOptifine
        },
        {
            "java.lang.NoSuchMethodError: 'void net.minecraft.server.level.DistanceManager.addRegionTicket(net.minecraft.server.level.TicketType, net.minecraft.world.level.ChunkPos, int, java.lang.Object, boolean)'",
            CrashCauses.IncompatibleForgeAndOptifine
        }
    };

    #region Regex

    [GeneratedRegex(@"(?<=\tBlock: Block\{)[^\}]+")]
    private static partial Regex BlockMatch();

    [GeneratedRegex(@"(?<=\tBlock location: World: )\([^\)]+\)")]
    private static partial Regex BlockLocationMatch();

    [GeneratedRegex(@"(?<=\tEntity Type: )[^\n]+(?= \()")]
    private static partial Regex EntityMatch();

    [GeneratedRegex(@"(?<=\tEntity's Exact location: )[^\n]+")]
    private static partial Regex EntityLocationMatch();

    [GeneratedRegex("(?<=Failed to create mod instance. ModID: )[^,]+")]
    private static partial Regex ModInstanceMatch1();

    [GeneratedRegex(@"(?<=Failed to create mod instance. ModId )[^\n]+(?= for )")]
    private static partial Regex ModInstanceMatch2();

    [GeneratedRegex(@"(?<=\]: Warnings were found! ?[\n]+)[\w\W]+?(?=[\n]+\[)")]
    private static partial Regex WarningsMatch();

    [GeneratedRegex("(?<=class \")[^']+(?=\"'s signer information)")]
    private static partial Regex PackSignerMatch();

    [GeneratedRegex(@"(?<=the game will display an error screen and halt[\s\S]+?Exception: )[\s\S]+?(?=\n\tat)")]
    private static partial Regex ForgeErrorMatch();

    [GeneratedRegex(@"(?<=A potential solution has been determined:\n)((\t)+ - [^\n]+\n)+")]
    private static partial Regex FabricSolutionMatch();

    [GeneratedRegex(@"(?<=\n\t[\w]+ : [A-Z]{1}:[^\n]+(/|\\))[^/\\\n]+?.jar", RegexOptions.IgnoreCase)]
    private static partial Regex GameModMatch1();

    [GeneratedRegex(@"Found a duplicate mod[^\n]+", RegexOptions.IgnoreCase)]
    private static partial Regex GameModMatch2();

    [GeneratedRegex(@"ModResolutionException: Duplicate[^\n]+", RegexOptions.IgnoreCase)]
    private static partial Regex GameModMatch3();

    [GeneratedRegex("(?<=in )[^./ ]+(?=.mixins.json.+failed injection check)")]
    private static partial Regex ModIdMatch1();

    [GeneratedRegex("(?<= failed .+ in )[^./ ]+(?=.mixins.json)")]
    private static partial Regex ModIdMatch2();

    [GeneratedRegex(@"(?<= in config \[)[^./ ]+(?=.mixins.json\] FAILED during )")]
    private static partial Regex ModIdMatch3();

    [GeneratedRegex("(?<= in callback )[^./ ]+(?=.mixins.json:)")]
    private static partial Regex ModIdMatch4();

    [GeneratedRegex(@"^[^\n.]+.\w+.[^\n]+\n\[$")]
    private static partial Regex MainClassMatch1();

    [GeneratedRegex(@"^\[[^\]]+\] [^\n.]+.\w+.[^\n]+\n\[")]
    private static partial Regex MainClassMatch2();

    [GeneratedRegex("(?<=Mod File: ).+")]
    private static partial Regex ModFileMatch();

    [GeneratedRegex(@"(?<=Failure message: )[\w\W]+?(?=\tMod)")]
    private static partial Regex ModLoaderMatch();

    [GeneratedRegex("(?<=Multiple entries with same key: )[^=]+")]
    private static partial Regex MultipleEntriesMatch();

    [GeneratedRegex("(?<=due to errors, provided by ')[^']+")]
    private static partial Regex ProvidedByMatch();

    [GeneratedRegex(@"(?<=LoaderExceptionModCrash: Caught exception from )[^\n]+")]
    private static partial Regex ModCausedCrashMatch();

    [GeneratedRegex(@"(?<=Failed loading config file .+ for modid )[^\n]+")]
    private static partial Regex ConfigFileMatch1();

    [GeneratedRegex("(?<=Failed loading config file ).+(?= of type)")]
    private static partial Regex ConfigFileMatch2();

    #endregion

    /// <summary>
    /// 分析日志
    /// </summary>
    public IEnumerable<CrashReport> AnalysisLogs() {
        GetAllLogs();

        var result = FuzzyProcessLogs()
            .Union(SpecificProcessGameLogs())
            .Union(SpecificProcessCrashLogs())
            .GroupBy(x => x.CrashCauses)
            .Select(y => y.First());

        foreach (var report in result) {
            yield return report;
        }
    }

    /// <summary>
    /// 获取所有日志
    /// </summary>
    private void GetAllLogs() {
        var gamePath = Path.Combine(_isIndependencyCore
            ? _gameEntry.OfVersionDirectoryPath(_isIndependencyCore)
            : _gameEntry.GameFolderPath);

        _gameLogs = ReadAllLine(Path.Combine(gamePath, "logs", "latest.log").ToFileInfo());

        var crashes = new List<FileInfo>();
        var crashReports = new DirectoryInfo(Path.Combine(gamePath, "crash-reports"));
        if (crashReports.Exists) {
            crashes.AddRange(crashReports.EnumerateFiles().Where(fi => fi.Extension is ".log" or ".txt"));
        }

        foreach (var item in crashes) {
            _crashLogs.AddRange(ReadAllLine(item));
        }

        string[] ReadAllLine(FileInfo file) {
            return file.Exists ? File.ReadAllLines(file.FullName) : default;
        }
    }

    /// <summary>
    /// 模糊处理日志
    /// </summary>
    private IEnumerable<CrashReport> FuzzyProcessLogs() {
        var allLogs = _gameLogs.Union(_crashLogs).ToImmutableArray();
        return allLogs.SelectMany(log => _logCrashCauses, (log, item) => new { log, item })
            .Where(t => t.log.Contains(t.item.Key))
            .Select(t => new CrashReport {
                Original = t.log,
                CrashCauses = t.item.Value
            });
    }

    /// <summary>
    /// 精确处理游戏日志
    /// </summary>
    private IEnumerable<CrashReport> SpecificProcessGameLogs() {
        foreach (var log in _gameLogs.ToImmutableArray()) {
            if (MainClassMatch1().IsMatch(log) || (MainClassMatch2().IsMatch(log) && !log.Contains("at net."))) {
                yield return new CrashReport {
                    Original = log,
                    CrashCauses = CrashCauses.IncorrectPathEncodingOrMainClassNotFound
                };
            }

            if (log.Contains("]: Warnings were found!")) {
                yield return new CrashReport {
                    Original = log,
                    CrashCauses = CrashCauses.FabricError,
                    Details = [WarningsMatch().Match(log).Value]
                };
            }

            if (log.Contains("Failed to create mod instance.")) {
                yield return new CrashReport {
                    Original = log,
                    CrashCauses = CrashCauses.ModInitFailed,
                    Details = [ModInstanceMatch1().Match(log).Value, ModInstanceMatch2().Match(log).Value]
                };
            }

            if (log.Contains("java.lang.NoSuchMethodError: net.minecraft.world.server.ChunkManager$ProxyTicketManager.shouldForceTicks(J)Z")
                && log.Contains("OptiFine")) {
                yield return new CrashReport {
                    Original = log,
                    CrashCauses = CrashCauses.FailedToLoadWorldBecauseOptiFine
                };
            }

            if (log.Contains("Could not reserve enough space")) {
                if (log.Contains("for 1048576KB object heap")) {
                    yield return new CrashReport {
                        Original = log,
                        CrashCauses = CrashCauses.NoEnoughMemory32
                    };

                } else {
                    yield return new CrashReport {
                        Original = log,
                        CrashCauses = CrashCauses.NoEnoughMemory
                    };
                }
            }

            if (log.Contains("signer information does not match signer information of other classes in the same package")) {
                yield return new CrashReport {
                    Original = log,
                    CrashCauses = CrashCauses.ContentValidationFailed,
                    Details = [PackSignerMatch().Match(log).Value]
                };
            }

            if (log.Contains("An exception was thrown, the game will display an error screen and halt.")) {
                yield return new CrashReport {
                    Original = log,
                    CrashCauses = CrashCauses.ForgeError,
                    Details = [ForgeErrorMatch().Match(log).Value]
                };
            }

            if (log.Contains("A potential solution has been determined:")) {
                yield return new CrashReport {
                    Original = log,
                    CrashCauses = CrashCauses.FabricErrorWithSolution,
                    Details = [FabricSolutionMatch().Match(log).Value]
                };
            }

            if (log.Contains("DuplicateModsFoundException")) {
                yield return new CrashReport {
                    Original = log,
                    CrashCauses = CrashCauses.DuplicateMod,
                    Details = [GameModMatch1().Match(log).Value]
                };
            }

            if (log.Contains("Found a duplicate mod")) {
                yield return new CrashReport {
                    Original = log,
                    CrashCauses = CrashCauses.DuplicateMod,
                    Details = [GameModMatch2().Match(log).Value]
                };
            }

            if (log.Contains("ModResolutionException: Duplicate")) {
                yield return new CrashReport {
                    Original = log,
                    CrashCauses = CrashCauses.DuplicateMod,
                    Details = [GameModMatch3().Match(log).Value]
                };
            }

            if (log.Contains("Mixin prepare failed ")
                || log.Contains("Mixin apply failed ")
                || log.Contains("mixin.injection.throwables.")
                || log.Contains(".mixins.json] FAILED during )")) {
                var modId = ModIdMatch1().Match(log).Value;

                if (string.IsNullOrEmpty(modId)) {
                    modId = ModIdMatch2().Match(log).Value;
                }

                if (string.IsNullOrEmpty(modId)) {
                    modId = ModIdMatch3().Match(log).Value;
                }

                if (string.IsNullOrEmpty(modId)) {
                    modId = ModIdMatch4().Match(log).Value;
                }

                yield return new CrashReport {
                    Original = log,
                    CrashCauses = CrashCauses.ModMixinFailed,
                    Details = string.IsNullOrEmpty(modId) ? null : [modId]
                };
            }
        }
    }

    /// <summary>
    /// 精确处理崩溃日志
    /// </summary>
    /// <returns></returns>
    private IEnumerable<CrashReport> SpecificProcessCrashLogs() {
        foreach (var log in _crashLogs.ToImmutableArray()) {
            if (log.Contains("-- MOD ")) {
                var modLogs = log.Split("-- MOD").Last();
                if (modLogs.Contains("Failure message: MISSING")) {
                    yield return new CrashReport {
                        Original = log,
                        CrashCauses = CrashCauses.ModLoaderError,
                        Details = [ModFileMatch().Match(log).Value],
                    };
                } else {
                    yield return new CrashReport {
                        Original = log,
                        CrashCauses = CrashCauses.ModCausedGameCrash,
                        Details = [ModLoaderMatch().Match(log).Value]
                    };
                }
            }

            if (log.Contains("Multiple entries with same key: ")) {
                yield return new CrashReport {
                    Original = log,
                    CrashCauses = CrashCauses.ModCausedGameCrash,
                    Details = [MultipleEntriesMatch().Match(log).Value]
                };
            }

            if (log.Contains("due to errors, provided by ")) {
                yield return new CrashReport {
                    Original = log,
                    CrashCauses = CrashCauses.ModCausedGameCrash,
                    Details = [ProvidedByMatch().Match(log).Value]
                };
            }

            if (log.Contains("LoaderExceptionModCrash: Caught exception from ")) {
                yield return new CrashReport {
                    Original = log,
                    CrashCauses = CrashCauses.ModCausedGameCrash,
                    Details = [ModCausedCrashMatch().Match(log).Value]
                };
            }

            if (log.Contains("Failed loading config file ")) {
                yield return new CrashReport {
                    Original = log,
                    CrashCauses = CrashCauses.ModCausedGameCrash,
                    Details = [ConfigFileMatch1().Match(log).Value, ConfigFileMatch2().Match(log).Value]
                };
            }

            if (log.Contains("Block location: World: ")) {
                yield return new CrashReport {
                    Original = log,
                    CrashCauses = CrashCauses.BlockCausedGameCrash,
                    Details = [BlockMatch().Match(log).Value, BlockLocationMatch().Match(log).Value]
                };
            }

            if (log.Contains("Entity's Exact location: ")) {
                yield return new CrashReport {
                    Original = log,
                    CrashCauses = CrashCauses.EntityCausedGameCrash,
                    Details = [EntityMatch().Match(log).Value, EntityLocationMatch().Match(log).Value]
                };
            }
        }
    }
}