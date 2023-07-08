using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using MinecraftLaunch.Modules.ArgumentsBuilders;
using MinecraftLaunch.Modules.Enum;
using MinecraftLaunch.Modules.Installer;
using MinecraftLaunch.Modules.Interface;
using MinecraftLaunch.Modules.Models.Launch;
using MinecraftLaunch.Modules.Toolkits;

namespace MinecraftLaunch.Launch {
    public sealed partial class JavaMinecraftLauncher : LauncherBase<JavaMinecraftArgumentsBuilder, MinecraftLaunchResponse> {
        public override async ValueTask<MinecraftLaunchResponse> LaunchTaskAsync(string id, Action<(float, string)> action) {
            IProgress<(float, string)> progress = new Progress<(float, string)>();
            ((Progress<(float, string)>)progress).ProgressChanged += ProgressChanged!;
            void ProgressChanged(object _, (float, string) e) => action(e);
            Process process = null;
            IEnumerable<string> args = new string[0];
            try {
                #region 预启动检查
                GameCore core = GameCoreToolkit.GetGameCore(id);

                progress.Report((0.2f, "正在查找游戏核心"));
                if (core == null) {
                    progress.Report((-1f, "启动失败，游戏核心不存在或已损坏"));
                    ((Progress<(float, string)>)progress).ProgressChanged -= ProgressChanged;
                    return await Task.FromResult(new MinecraftLaunchResponse(null, LaunchState.Failed, null, new Exception("启动失败，游戏核心不存在或已损坏")));
                }

                progress.Report((0.4f, "正在检查 Jvm 配置"));
                if (LaunchSetting.JvmConfig == null) {
                    progress.Report((-1f, "启动失败，未配置 Jvm 信息"));
                    ((Progress<(float, string)>)progress).ProgressChanged -= ProgressChanged;
                    return await Task.FromResult(new MinecraftLaunchResponse(null, LaunchState.Failed, null, new Exception("启动失败，未配置 Jvm 信息")));
                }

                if (!LaunchSetting.JvmConfig.JavaPath.Exists) {
                    progress.Report((-1f, "启动失败，Java 路径不存在或已损坏"));
                    ((Progress<(float, string)>)progress).ProgressChanged -= ProgressChanged;
                    return await Task.FromResult(new MinecraftLaunchResponse(null, LaunchState.Failed, null, new Exception("启动失败，Java 路径不存在或已损坏")));
                }

                progress.Report((0.5f, "正在验证账户信息"));
                if (LaunchSetting.Account == null) {
                    progress.Report((-1f, "启动失败，未设置账户"));
                    ((Progress<(float, string)>)progress).ProgressChanged -= ProgressChanged;
                    return await Task.FromResult(new MinecraftLaunchResponse(null, LaunchState.Failed, null, new Exception("启动失败，未设置账户")));
                }

                progress.Report((0.6f, "正在检查游戏依赖文件"));
                await new ResourceInstaller(core).DownloadAsync(delegate (string x, float a) {
                    progress.Report((0.6f + a * 0.8f, "正在下载游戏依赖文件：" + x));
                });
                progress.Report((0.8f, "正在构建启动参数"));
                ArgumentsBuilder = new JavaMinecraftArgumentsBuilder(core, LaunchSetting);
                args = ArgumentsBuilder.Build();
                progress.Report((9f, "正在检查Natives"));
                DirectoryInfo natives = new DirectoryInfo((LaunchSetting.NativesFolder != null && LaunchSetting.NativesFolder.Exists) ? LaunchSetting.NativesFolder.FullName.ToString() : Path.Combine(core.Root.FullName, "versions", core.Id, "natives"));
                try {
                    ZipToolkit.GameNativesDecompress(natives, core.LibraryResources);
                }
                catch (Exception ex2) when (ex2.Message.Contains("The process cannot access the file")) {
                    progress.Report((0.95f, "Natives文件情况：已解压，无需再次解压"));
                }
                catch {
                    throw;
                }
                #endregion

                #region 启动
                progress.Report((1f, "正在尝试启动游戏"));
                process = new Process {
                    StartInfo = new ProcessStartInfo {
                        FileName = LaunchSetting.JvmConfig.JavaPath.FullName,
                        Arguments = string.Join(' '.ToString(), args),
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        WorkingDirectory = ((LaunchSetting.IsEnableIndependencyCore && LaunchSetting.WorkingFolder != null) ? 
                        ((LaunchSetting.IsEnableIndependencyCore && LaunchSetting.WorkingFolder.Exists) ?
                        LaunchSetting.WorkingFolder.FullName : core.Root.FullName) : core.Root.FullName)
                    },
                    EnableRaisingEvents = true
                };
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                await Task.Delay(500);

                ((Progress<(float, string)>)progress).ProgressChanged -= ProgressChanged;
                return new MinecraftLaunchResponse(process, LaunchState.Succeess, args) {
                    RunTime = stopWatch
                };
                #endregion
            }
            catch (Exception ex) {
                if (ex.GetType() == typeof(OperationCanceledException)) {
                    ((Progress<(float, string)>)progress).ProgressChanged -= ProgressChanged;
                    return new MinecraftLaunchResponse(process, LaunchState.Cancelled, args);
                }
                ((Progress<(float, string)>)progress).ProgressChanged -= ProgressChanged;
                return new MinecraftLaunchResponse(process, LaunchState.Failed, args, ex);
            }
        }

        public async ValueTask<MinecraftLaunchResponse> LaunchTaskAsync(string id) {
            Process process = null;
            IEnumerable<string> args = new string[0];
            try {
                GameCore core = GameCoreToolkit.GetGameCore(id);
                if (core == null)
                    return await Task.FromResult(new MinecraftLaunchResponse(null, LaunchState.Failed, null, new Exception("启动失败，游戏核心不存在或已损坏")));
                if (LaunchSetting.JvmConfig == null)
                    return await Task.FromResult(new MinecraftLaunchResponse(null, LaunchState.Failed, null, new Exception("启动失败，未配置 Jvm 信息")));
                if (!LaunchSetting.JvmConfig.JavaPath.Exists)
                    return await Task.FromResult(new MinecraftLaunchResponse(null, LaunchState.Failed, null, new Exception("启动失败，Java 路径不存在或已损坏")));
                if (LaunchSetting.Account == null)
                    return await Task.FromResult(new MinecraftLaunchResponse(null, LaunchState.Failed, null, new Exception("启动失败，未设置账户")));
                ArgumentsBuilder = new JavaMinecraftArgumentsBuilder(core, LaunchSetting);
                args = ArgumentsBuilder.Build();
                await new ResourceInstaller(core).DownloadAsync(delegate {

                });
                DirectoryInfo natives = new DirectoryInfo((LaunchSetting.NativesFolder != null && LaunchSetting.NativesFolder.Exists) ? LaunchSetting.NativesFolder.FullName.ToString() : Path.Combine(core.Root.FullName, "versions", core.Id, "natives"));
                try {
                    ZipToolkit.GameNativesDecompress(natives, core.LibraryResources);
                }
                catch (Exception ex2) when (ex2.Message.Contains("The process cannot access the file")) { }
                catch { throw; }
                process = new Process {
                    StartInfo = new ProcessStartInfo {
                        FileName = LaunchSetting.JvmConfig.JavaPath.FullName,
                        Arguments = string.Join(' ', args),
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        WorkingDirectory = ((LaunchSetting.IsEnableIndependencyCore && LaunchSetting.WorkingFolder != null)
                        ? ((LaunchSetting.IsEnableIndependencyCore && LaunchSetting.WorkingFolder.Exists) 
                        ? LaunchSetting.WorkingFolder.FullName : core.Root.FullName) : core.Root.FullName)
                    },
                    EnableRaisingEvents = true
                };
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                return new MinecraftLaunchResponse(process, LaunchState.Succeess, args) {
                    RunTime = stopWatch
                };
            }
            catch (Exception ex) {
                if (ex.GetType() == typeof(OperationCanceledException)) {
                    return new MinecraftLaunchResponse(process, LaunchState.Cancelled, args);
                }
                return new MinecraftLaunchResponse(process, LaunchState.Failed, args, ex);
            }
        }

        public MinecraftLaunchResponse Launch(string id, Action<(float, string)> action) {
            return LaunchTaskAsync(id, action).GetAwaiter().GetResult();
        }

        public MinecraftLaunchResponse Launch(string id) {
            return LaunchTaskAsync(id).GetAwaiter().GetResult();
        }
    }

    partial class JavaMinecraftLauncher {
        public JavaMinecraftLauncher() { }

        public JavaMinecraftLauncher(LaunchConfig launchSetting, GameCoreToolkit gameCoreToolkit) {
            LaunchSetting = launchSetting;
            GameCoreToolkit = gameCoreToolkit;
            if (LaunchSetting.Account == null)
                throw new ArgumentNullException("LaunchSetting.Account");
        }
    }

    partial class JavaMinecraftLauncher {
        public override LaunchConfig LaunchSetting { get; set; }

        public override JavaMinecraftArgumentsBuilder ArgumentsBuilder { get; set; }

        public GameCoreToolkit GameCoreToolkit { get; set; }
    }
}