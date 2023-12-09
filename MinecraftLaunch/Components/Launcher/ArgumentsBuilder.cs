using MinecraftLaunch.Classes.Enums;
using MinecraftLaunch.Classes.Models.Game;
using MinecraftLaunch.Classes.Models.Launch;
using MinecraftLaunch.Components.Resolver;
using MinecraftLaunch.Extensions;
using MinecraftLaunch.Utilities;

namespace MinecraftLaunch.Components.Launcher {
    class ArgumentsBuilder(GameEntry gameEntity, LaunchConfig launchConfig) {
        public GameEntry GameEntry => gameEntity;

        public LaunchConfig LaunchConfig => launchConfig;

        public IEnumerable<string> DefaultAdvancedArguments =>
            ["-XX:-OmitStackTraceInFastThrow",
                "-XX:-DontCompileHugeMethods",
                "-Dfile.encoding=GB18030",
                "-Dfml.ignoreInvalidMinecraftCertificates=true",
                "-Dfml.ignorePatchDiscrepancies=true",
                "-Djava.rmi.server.useCodebaseOnly=true",
                "-Dcom.sun.jndi.rmi.object.trustURLCodebase=false",
                "-Dcom.sun.jndi.cosnaming.object.trustURLCodebase=false"];

        public IEnumerable<string> DefaultGCArguments =>
            ["-XX:+UseG1GC",
                "-XX:+UnlockExperimentalVMOptions",
                "-XX:G1NewSizePercent=20",
                "-XX:G1ReservePercent=20",
                "-XX:MaxGCPauseMillis=50",
                "-XX:G1HeapRegionSize=16m",
                "-XX:-UseAdaptiveSizePolicy"];

        public IEnumerable<string> Build() {
            foreach (string frontArgument in GetFrontArguments()) {
                yield return frontArgument;
            }

            yield return GameEntry.MainClass;

            foreach (string behindArgument in GetBehindArguments()) {
                yield return behindArgument;
            }
        }

        public IEnumerable<string> GetBehindArguments() {
            var keyValuePairs = new Dictionary<string, string>() {
                { "${user_properties}", "{}" },
                { "${version_name}", GameEntry.Id },
                { "${version_type}", LaunchConfig.LauncherName },
                { "${auth_player_name}", LaunchConfig.Account.Name },
                { "${auth_session}", LaunchConfig.Account.AccessToken },
                { "${auth_uuid}", LaunchConfig.Account.Uuid.ToString("N") },
                { "${auth_access_token}", LaunchConfig.Account.AccessToken },
                { "${assets_root}", Path.Combine(GameEntry.GameFolderPath, "assets").ToPath() },
                { "${game_assets}", Path.Combine(GameEntry.GameFolderPath, "assets").ToPath() },
                { "${assets_index_name}", Path.GetFileNameWithoutExtension(GameEntry.AssetsIndexJsonPath) },
                { "${user_type}", LaunchConfig.Account.Type.Equals(AccountType.Microsoft) ? "MSA" : "Mojang" },
                { "${game_directory}", GameEntry.OfVersionDirectoryPath(LaunchConfig.IsEnableIndependencyCore) },
            };

            var args = new List<string>(GameEntry.BehindArguments);

            if (LaunchConfig.GameWindowConfig != null) {
                args.Add($"--width {LaunchConfig.GameWindowConfig.Width}");
                args.Add($"--height {LaunchConfig.GameWindowConfig.Height}");
                if (LaunchConfig.GameWindowConfig.IsFullscreen) {
                    args.Add("--fullscreen");
                }
            }

            if (LaunchConfig.ServerConfig != null && !string.IsNullOrEmpty(LaunchConfig.ServerConfig.Ip) 
                && LaunchConfig.ServerConfig.Port != 0) {
                args.Add($"--server {LaunchConfig.ServerConfig.Ip}");
                args.Add($"--port {LaunchConfig.ServerConfig.Port}");
            }

            return args.Select(arg => arg.Replace(keyValuePairs));
        }

        public IEnumerable<string> GetFrontArguments() {
            var keyValuePairs = new Dictionary<string, string>() {
                { "${launcher_name}", "MinecraftLaunch" },
                { "${launcher_version}", "3" },
                { "${classpath_separator}", Path.PathSeparator.ToString() },
                { "${classpath}", GetClasspath().ToPath() },
                { "${client}", GameEntry.JarPath.ToPath() },
                { "${min_memory}", LaunchConfig.JvmConfig.MinMemory.ToString() },
                { "${max_memory}", LaunchConfig.JvmConfig.MaxMemory.ToString() },
                { "${library_directory}", Path.Combine(GameEntry.GameFolderPath, "libraries").ToPath() },
                { "${version_name}", GameEntry.InheritsFrom is null ? GameEntry.Id : GameEntry.InheritsFrom.Id },
                { "${natives_directory}", GetNativesDirectory() }
            };

            if (!Directory.Exists(keyValuePairs["${natives_directory}"])) {
                Directory.CreateDirectory(keyValuePairs["${natives_directory}"].Trim('"'));
            }

            var args = new List<string> {
                "-Xmn${min_memory}m",
                "-Xmx${max_memory}m",
                "-Dminecraft.client.jar=${client}",
                "-Dlog4j2.formatMsgNoLookups=true"
            };

            args.AddRange(GetEnvironmentJvmArguments());
            args.AddRange(LaunchConfig.JvmConfig.GCArguments ?? DefaultGCArguments);
            args.AddRange(LaunchConfig.JvmConfig.AdvancedArguments ?? DefaultAdvancedArguments);
            args.AddRange(GameEntry.FrontArguments);

            foreach (string arg in args) {
                yield return arg.Replace(keyValuePairs);
            }
        }

        private string GetClasspath() {
            var libraries = new LibrariesResolver(gameEntity)
                .GetLibraries();

            var classPath = string.Join(Path.PathSeparator,
                libraries.Select(lib => lib.Path));

            if (!string.IsNullOrEmpty(gameEntity.JarPath)) {
                classPath += Path.PathSeparator + gameEntity.JarPath;
            }

            return classPath;
        }

        private string GetNativesDirectory() {
            return LaunchConfig.NativesFolder != null && LaunchConfig.NativesFolder.Exists
                ? LaunchConfig.NativesFolder.FullName.ToString()
                : Path.Combine(GameEntry.OfVersionDirectoryPath(LaunchConfig.IsEnableIndependencyCore), "natives").ToPath();
        }

        private static IEnumerable<string> GetEnvironmentJvmArguments() {
            Platform platformName = EnvironmentUtil.GetPlatformName();
            if (!(platformName == Platform.windows)) {
                if (platformName == Platform.osx)
                    yield return "-XstartOnFirstThread";
            } else {
                yield return "-XX:HeapDumpPath=MojangTricksIntelDriversForPerformance_javaw.exe_minecraft.exe.heapdump";
                if (Environment.OSVersion.Version.Major == 10) {
                    yield return "-Dos.name=\"Windows 10\"";
                    yield return "-Dos.version=10.0";
                }
            }
            if (EnvironmentUtil.Arch == "32")
                yield return "-Xss1M";
        }
    }
}
