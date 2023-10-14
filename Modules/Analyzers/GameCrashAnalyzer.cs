using MinecraftLaunch.Modules.Enum;
using MinecraftLaunch.Modules.Interface;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace MinecraftLaunch.Modules.Analyzers {
    /// <summary>
    /// 游戏崩溃分析器
    /// </summary>
    public partial class GameCrashAnalyzer : IAnalyzer<Dictionary<CrashReason, List<string>>> {
        /// <summary>
        /// 崩溃分析方法
        /// </summary>
        public async ValueTask<Dictionary<CrashReason, List<string>>> AnalyseAsync() {
            Trace.WriteLine("[Crash] 开始分析崩溃原因");
            var log = Log.ToLower();

            RoughLogMatching();
            if (CrashReason.Count > 0) goto Done;

            if (log.Contains("forge") || log.Contains("fabric") || log.Contains("liteloader")) {
                if (!string.IsNullOrEmpty(Log)) {
                    var stacks = Log.Replace("System Details", "¨").Split("¨").First();
                    var keywords = AnalyzeStackKeyword(stacks);
                    if (keywords.Count > 0) {
                        var names = TryAnalyzeModOfCrash(keywords);
                        if (names is not null && names.Count > 0)
                            AddPossibleCauses(Enum.CrashReason.MCLogStackAnalysisFoundKeyword, keywords);
                        else
                            AddPossibleCauses(Enum.CrashReason.CrashLogStackAnalysisFoundModName, names);
                    }
                }

                var fatals = Regex.Matches(Log, @"/FATAL] [\w\W]+?(?=[\n]+\[)").Select(x => x.Value).ToList();
                if (fatals.Count > 0) {
                    var keywords = new List<string>();
                    foreach (var item in fatals)
                        keywords.AddRange(AnalyzeStackKeyword(item));

                    if (keywords.Count > 0) {
                        AddPossibleCauses(Enum.CrashReason.MCLogStackAnalysisFoundKeyword, fatals.Distinct().ToList());
                        goto Done;
                    }
                }
            }

            AccurateLogMatching();
        Done:
            if (CrashReason.Count != 0) {
                CrashReason.Keys.ToList().ForEach(x => Trace.WriteLine($"[Crash]  - {x}"));
                return CrashReason;
            }
            return null;
        }

        /// <summary>
        /// 日志精准匹配方法
        /// </summary>
        private void AccurateLogMatching() {
            if (Log.Contains("]: Warnings were found!"))
                AddPossibleCauses(Enum.CrashReason.FabricError);

            if (Log.Contains("\tBlock location: World: "))
                AddPossibleCauses(Enum.CrashReason.SpecificBlockCausedCrash);

            if (Log.Contains("\tEntity's Exact location: "))
                AddPossibleCauses(Enum.CrashReason.SpecificEntityCausedCrash);

            if (Log.Contains("Couldn't load texture") || Log.Contains("Could not load image"))
                AddPossibleCauses(Enum.CrashReason.UnableToLoadTexture);

            if (Log.Contains("UnsupportedClassVersionError"))
                AddPossibleCauses(Enum.CrashReason.UnsupportedJavaClassVersionError);
        }

        /// <summary>
        /// 日志粗略匹配方法
        /// </summary>
        private void RoughLogMatching() {
            if (string.IsNullOrEmpty(Log))
                throw new ArgumentNullException("没有任何日志，已中止崩溃分析");

            if (Log.Contains("Unable to make protected final java.lang.Class java.lang.ClassLoader.defineClass") ||
                Log.Contains("Unsupported class file major version") || Log.Contains("because module java.base does not export") ||
                Log.Contains("java.lang.ClassNotFoundException: jdk.nashorn.api.scripting.NashornScriptEngineFactory") ||
                Log.Contains("java.lang.ClassNotFoundException: java.lang.invoke.LambdaMetafactory"))
                AddPossibleCauses(Enum.CrashReason.JavaVersionTooHigh);

            if (Log.Contains("Found multiple arguments for option fml.forgeVersion, but you asked for only one"))
                AddPossibleCauses(Enum.CrashReason.MultipleForgeInVersionJson);

            if (Log.Contains("The driver does not appear to support OpenGL"))
                AddPossibleCauses(Enum.CrashReason.GraphicsCardDoesNotSupportOpenGL);

            if (Log.Contains("java.lang.ClassCastException: java.base/jdk") || Log.Contains("java.lang.ClassCastException: class jdk"))
                AddPossibleCauses(Enum.CrashReason.UsingJDK);

            if (Log.Contains("Cannot read field \"ofTelemetry\" because \"net.optifine.Config.gameSettings\" is null") || Log.Contains("TRANSFORMER/net.optifine/net.optifine.reflect.Reflector.<clinit>(Reflector.java"))
                AddPossibleCauses(Enum.CrashReason.OptiFineIncompatibleWithForge);

            if (Log.Contains("Open J9 is not supported") || Log.Contains("OpenJ9 is incompatible") || Log.Contains(".J9VMInternals."))
                AddPossibleCauses(Enum.CrashReason.UsingOpenJ9);

            if (Log.Contains("The directories below appear to be extracted jar files. Fix this before you continue.") ||
                Log.Contains("Extracted mod jars found, loading will NOT continue"))
                AddPossibleCauses(Enum.CrashReason.ModFileDecompressed);

            if (Log.Contains("java.lang.OutOfMemoryError"))
                AddPossibleCauses(Enum.CrashReason.InsufficientMemory);

            if (Log.Contains("java.lang.NoSuchMethodError: sun.security.util.ManifestEntryVerifier"))
                AddPossibleCauses(Enum.CrashReason.LowVersionForgeIncompatibleWithHighVersionJava);

            if (Log.Contains("1282: Invalid operation"))
                AddPossibleCauses(Enum.CrashReason.ShaderOrResourcePackCausedOpenGL1282Error);

            if (Log.Contains("signer information does not match signer information of other classes in the same package"))
                AddPossibleCauses(Enum.CrashReason.FileOrContentCheckFailed);

            if (Log.Contains("An exception was thrown, the game will display an error screen and halt."))
                AddPossibleCauses(Enum.CrashReason.ForgeError);

            if (Log.Contains("A potential solution has been determined:"))
                AddPossibleCauses(Enum.CrashReason.FabricErrorWithSolution);

            if (Log.Contains("Maybe try a lower resolution resourcepack?"))
                AddPossibleCauses(Enum.CrashReason.TextureTooLargeOrInsufficientGraphicsConfig);

            if (Log.Contains("java.lang.NoSuchMethodError: net.minecraft.world.server.ChunkManager$ProxyTicketManager.shouldForceTicks(J)Z") && Log.Contains("OptiFine"))
                AddPossibleCauses(Enum.CrashReason.OptiFineCausedWorldLoadingFailure);

            if (Log.Contains("Could not reserve enough space"))
                if (Log.Contains("for 1048576KB object heap"))
                    AddPossibleCauses(Enum.CrashReason.Using32BitJavaCausedInsufficientJVMMemory);
                else AddPossibleCauses(Enum.CrashReason.InsufficientMemory);

            if (Log.Contains("DuplicateModsFoundException") || Log.Contains("Found a duplicate mod") || Log.Contains("ModResolutionException: Duplicate"))
                AddPossibleCauses(Enum.CrashReason.ModInstalledRepeatedly);

            if (Regex.IsMatch(Log, "(?<=in )[^./ ]+(?=.mixins.json.+failed injection check)") || Log.Contains("mixin.injection.throwables.") || Log.Contains(".mixins.json] FAILED during )")) {
                var mod = Regex.Match(Log, "(?<=in )[^./ ]+(?=.mixins.json.+failed injection check)").Value;
                if (string.IsNullOrEmpty(mod))
                    mod = Regex.Match(Log, "(?<= failed .+ in )[^./ ]+(?=.mixins.json)").Value;

                if (string.IsNullOrEmpty(mod))
                    mod = Regex.Match(Log, @"(?<= in config \[)[^./ ]+(?=.mixins.json\] FAILED during )").Value;

                if (string.IsNullOrEmpty(mod))
                    mod = Regex.Match(Log, "(?<= in callback )[^./ ]+(?=.mixins.json:)").Value;
                AddPossibleCauses(Enum.CrashReason.ModMixinFailed, TryAnalyModName(mod.TrimEnd(("\r\n" + " ").ToCharArray())));
            }

            if (Log.Contains("Caught exception from "))
                AddPossibleCauses(Enum.CrashReason.ModCausedGameCrash, TryAnalyModName(Regex.Match(Log, "[^\n]+?(?)").Value.TrimEnd('\r', '\n', ' ')));

            if (Log.Contains("Failed to create mod instance."))
                AddPossibleCauses(Enum.CrashReason.ModInitializationFailed, TryAnalyModName((Regex.IsMatch(Log, "(?<=Failed to create mod instance. ModID: )[^,]+") ? Regex.Match(Log, "(?<=Failed to create mod instance. ModID: )[^,]+").Value : Regex.Match(Log, @"(?<=Failed to create mod instance. ModId )[^\n]+(?= for )").Value).TrimEnd('\r', '\n')));

            if (Log.Contains("maximum id range exceeded"))
                AddPossibleCauses(Enum.CrashReason.TooManyModsExceededIDLimit);

            if (Log.Contains("java.lang.OutOfMemoryError"))
                AddPossibleCauses(Enum.CrashReason.InsufficientMemory);

            if (Log.Contains("Manually triggered debug crash"))
                AddPossibleCauses(Enum.CrashReason.PlayerTriggeredDebugCrash);

            if (Log.Contains("-- MOD ")) {
                var loglast = Log.Split("-- MOD").Last();
                if (loglast.Contains("Failure message: MISSING"))
                    AddPossibleCauses(Enum.CrashReason.ModCausedGameCrash);
                else AddPossibleCauses(Enum.CrashReason.ModLoaderError);
            }
        }

        /// <summary>
        /// 尝试分析导致崩溃的模组列表
        /// </summary>
        private List<string> TryAnalyzeModOfCrash(List<string> keywords) {
            var mods = new List<string>();

            //预处理关键字，也就是分割括号
            var realkeywords = new List<string>();
            foreach (var i in keywords)
                foreach (var v in i.Split("("))
                    realkeywords.Add(v.Trim(' ', ')'));
            keywords = realkeywords;

            if (!Log.Contains("A detailed walkthrough of the error"))
                return Array.Empty<string>().ToList();

            var details = Log.Replace("A detailed walkthrough of the error", "¨");
            var isfabricmod = details.Contains("Fabric Mods");
            if (isfabricmod)
                details = details.Replace("Fabric Mods", "¨");
            details = details.Split("¨").Last();

            //The FoegeMod is get all has the ".jar" lines and
            //the fabricmod is get all has the "Mod" lines.
            //by xilu
            var modIdlines = new List<string>();
            foreach (var item in details.Split('\n'))
                if (item.ToLower().Contains(".jar") || isfabricmod && item.StartsWith("\t" + "\t") && !Regex.IsMatch(item, @"\t\tfabric[\w-]*: Fabric"))
                    modIdlines.Add(item);

            var hintlines = new List<string>();
            foreach (var item in keywords) {
                foreach (var i in modIdlines) {
                    var realmod = i.ToLower().Replace("_", "");
                    if (!realmod.Contains(item.ToLower().Replace("_", ""))) continue;
                    if (realmod.Contains("minecraft.jar") || realmod.Contains(" forge-"))
                        continue;
                    hintlines.Add(i.Trim("\r\n".ToCharArray()));
                    break;
                }
            }
            hintlines = hintlines.Distinct().ToList();
            hintlines.ForEach(x => Trace.WriteLine($"[Crash] - {x}"));

            //fuck the regex 正则我日你仙人
            foreach (var line in hintlines) {
                var name = string.Empty;
                if (isfabricmod)
                    name = Regex.Match(line, @"(?<=: )[^\n]+(?= [^\n]+)").Value;
                else name = Regex.Match(line, @"(?<=\()[^\t]+.jar(?=\))|(?<=(\t\t)|(\| ))[^\t\|]+.jar", RegexOptions.IgnoreCase).Value;
                if (!string.IsNullOrEmpty(name)) mods.Add(name);
            }

            return mods.Count is 0 ? Array.Empty<string>().ToList() : mods;
        }

        /// <summary>
        /// 尝试从堆栈中提取 Mod Id 关键字
        /// </summary>
        /// <returns></returns>
        private List<string> AnalyzeStackKeyword(string errorstack) {
            var stacksearchres = Regex.Matches((string.IsNullOrEmpty(errorstack) ? string.Empty : errorstack) + "\r\n", @"(?<=\n[^{]+)[a-zA-Z]+\w+\.[a-zA-Z]+[\w\.]+(?=\.[\w\.$]+\.)").Select(x => x.Value).ToList();
            var Possiblestacks = new List<string>();
            foreach (var i in stacksearchres) {
                foreach (var stack in new string[] { "java", "sun", "javax", "jdk", "oolloo",
                "org.lwjgl", "com.sun", "net.minecraftforge", "com.mojang", "net.minecraft", "cpw.mods", "com.google", "org.apache", "org.spongepowered", "net.fabricmc", "com.mumfrey",
                "com.electronwill.nightconfig", "it.unimi.dsi",
                "MojangTricksIntelDriversForPerformance_javaw" })
                    if (i.StartsWith(stack)) goto NextStack;
                Possiblestacks.Add(i.Trim());
            NextStack:
                Trace.Write("");
            }
            Possiblestacks = Possiblestacks.Distinct().ToList();
            Trace.WriteLine($"[Crash] 找到 {Possiblestacks.Count} 条可能的堆栈信息");
            if (Possiblestacks.Count is 0) return new();
            Possiblestacks.ForEach(x => Trace.WriteLine($"[Crash] - {x}"));

            var possiblewords = new List<string>();
            foreach (var item in Possiblestacks) {
                var splited = item.Split('.');
                for (int i = 0; i < Math.Min(3, splited.Count() - 1); i++) {
                    var word = splited[i];
                    if (word.Length <= 2 || word.StartsWith("func_")) continue;
                    if (new string[] {"com", "org", "net", "asm", "fml", "mod", "jar", "sun", "lib", "map", "gui", "dev", "nio", "api", "dsi",
                    "core", "init", "mods", "main", "file", "game", "load", "read", "done", "util", "tile", "item", "base",
                    "forge", "setup", "block", "model", "mixin", "event", "unimi",
                    "common", "server", "config", "loader", "launch", "entity", "assist", "client", "modapi", "mojang", "shader", "events", "github", "recipe",
                    "preinit", "preload", "machine", "reflect", "channel", "general", "handler", "content",
                    "fastutil", "optifine", "minecraft", "transformers", "universal", "internal", "multipart", "minecraftforge", "override", "blockentity"
                    }.Contains(word.ToLower())) continue;
                    possiblewords.Add(word.Trim());
                }
            }
            possiblewords = possiblewords.Distinct().ToList();
            Trace.WriteLine($"[Crash] 从堆栈信息中找到 {possiblewords.Count} 个可能的 Mod ID 关键词");
            if (possiblewords.Count > 0)
                Trace.WriteLine($"[Crash] - {string.Join(", ", possiblewords)}");
            if (possiblewords.Count > 10) {
                Trace.WriteLine("[Crash] 关键词过多，考虑匹配出错，不纳入考虑");
                return new();
            } else return possiblewords;
        }

        /// <summary>
        /// 向集合插入一个可能的崩溃原因，如果已有将不会插入
        /// </summary>
        private void AddPossibleCauses(CrashReason reason, ICollection<string> strings = null) {
            if (CrashReason.ContainsKey(reason)) {
                if (strings is not null) {
                    CrashReason[reason].AddRange(strings);
                    CrashReason[reason].Distinct();
                }
            } else CrashReason.Add(reason, new List<string>());
        }

        /// <summary>
        /// 尝试获取 mod 名称
        /// </summary>
        /// <param name="keymods"></param>
        /// <returns></returns>
        private List<string> TryAnalyModName(string keymods) {
            var raw = new List<string>() { keymods };
            if (string.IsNullOrEmpty(keymods)) return raw;
            return TryAnalyzeModOfCrash(raw) is not null ? TryAnalyzeModOfCrash(raw) : raw;
        }
    }

    partial class GameCrashAnalyzer {
        /// <summary>
        /// 崩溃日志
        /// </summary>
        public string Log { get; set; }
        /// <summary>
        /// 导致崩溃的可能原因
        /// </summary>
        public Dictionary<CrashReason, List<string>> CrashReason { get; set; } = new();
    }

    partial class GameCrashAnalyzer {
        public GameCrashAnalyzer(IEnumerable<string> log) {
            Log = string.Join(' ', log);
        }
    }
}
