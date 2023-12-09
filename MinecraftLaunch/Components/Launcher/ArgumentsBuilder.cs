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
            var keyValuePairs = new Dictionary<string, string>()
            {
                { "${user_properties}" , "{}" },
                { "${version_name}" , GameEntry.Id },
                { "${version_type}" , LaunchConfig.LauncherName },
                { "${auth_player_name}" , LaunchConfig.Account.Name },
                { "${auth_session}" , LaunchConfig.Account.AccessToken },
                { "${auth_uuid}" , LaunchConfig.Account.Uuid.ToString("N") },
                { "${auth_access_token}" , LaunchConfig.Account.AccessToken },
                { "${assets_root}" , Path.Combine(GameEntry.GameFolderPath, "assets").ToPath() },
                { "${game_assets}" , Path.Combine(GameEntry.GameFolderPath, "assets").ToPath() },
                { "${assets_index_name}" , Path.GetFileNameWithoutExtension(GameEntry.AssetsIndexJsonPath) },
                { "${user_type}" , LaunchConfig.Account.Type.Equals(AccountType.Microsoft) ? "MSA" : "Mojang" },
                { "${game_directory}" , GameEntry.OfVersionDirectoryPath(LaunchConfig.IsEnableIndependencyCore) },
            };

            List<string> list = GameEntry.BehindArguments
                .ToList();

            if (LaunchConfig.GameWindowConfig != null) {
                list.Add($"--width {LaunchConfig.GameWindowConfig.Width}");
                list.Add($"--height {LaunchConfig.GameWindowConfig.Height}");
                if (LaunchConfig.GameWindowConfig.IsFullscreen) {
                    list.Add("--fullscreen");
                }
            }

            if (LaunchConfig.ServerConfig != null && !string.IsNullOrEmpty(LaunchConfig.ServerConfig.Ip) && LaunchConfig.ServerConfig.Port != 0) {
                list.Add("--server " + LaunchConfig.ServerConfig.Ip);
                list.Add("--port " + LaunchConfig.ServerConfig.Port);
            }

            foreach (string item in list) {
                yield return item.Replace(keyValuePairs);
            }
        }

        public IEnumerable<string> GetFrontArguments() {
            var keyValuePairs = new Dictionary<string, string>()
            {
                { "${launcher_name}", "MinecraftLaunch" },
                { "${launcher_version}", "3" },
                { "${classpath_separator}", Path.PathSeparator.ToString() },
                { "${classpath}", GetClasspath().ToPath() },
                { "${client}", GameEntry.JarPath.ToPath() },
                { "${min_memory}", LaunchConfig.JvmConfig.MinMemory.ToString() },
                { "${max_memory}", LaunchConfig.JvmConfig.MaxMemory.ToString() },
                { "${library_directory}", Path.Combine(GameEntry.GameFolderPath, "libraries").ToPath() },
                {
                    "${version_name}", GameEntry.InheritsFrom is null
                    ? GameEntry.Id
                    : GameEntry.InheritsFrom.Id
                },
                {
                    "${natives_directory}",
                    LaunchConfig.NativesFolder != null && LaunchConfig.NativesFolder.Exists
                    ? LaunchConfig.NativesFolder.FullName.ToString()
                    : Path.Combine(GameEntry.OfVersionDirectoryPath(LaunchConfig.IsEnableIndependencyCore),"natives").ToPath()
                }
            };

            if (!Directory.Exists(keyValuePairs["${natives_directory}"])) {
                Directory.CreateDirectory(keyValuePairs["${natives_directory}"].Trim('"'));
            }

            List<string> args = ["-Xmn${min_memory}m", "-Xmx${max_memory}m", "-Dminecraft.client.jar=${client}"];

            foreach (string item4 in GetEnvironmentJvmArguments())
                args.Add(item4);

            if (LaunchConfig.JvmConfig.GCArguments == null)
                DefaultGCArguments.ToList().ForEach(x => args.Add(x));
            else
                LaunchConfig.JvmConfig.GCArguments.ToList().ForEach(x => args.Add(x));

            if (LaunchConfig.JvmConfig.AdvancedArguments == null)
                DefaultAdvancedArguments.ToList().ForEach(x => args.Add(x));
            else
                LaunchConfig.JvmConfig.AdvancedArguments.ToList().ForEach(x => args.Add(x));

            args.Add("-Dlog4j2.formatMsgNoLookups=true");
            foreach (string item3 in GameEntry.FrontArguments) {
                args.Add(item3);
            }

            foreach (string item2 in args) {
                yield return item2.Replace(keyValuePairs);
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

        private static IEnumerable<string> GetEnvironmentJvmArguments() {
            string platformName = EnvironmentUtil.GetPlatformName();
            if (!(platformName == "windows")) {
                if (platformName == "osx")
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
