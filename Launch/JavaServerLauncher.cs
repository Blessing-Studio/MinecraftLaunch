using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using MinecraftLaunch.Modules.ArgumentsBuilders;
using MinecraftLaunch.Modules.Enum;
using MinecraftLaunch.Modules.Interface;
using MinecraftLaunch.Modules.Models.Launch;

namespace MinecraftLaunch.Launch
{
    public partial class JavaServerLauncher : LauncherBase<JavaServerArgumentsBuilder, JavaServerLaunchResponse>
    {
        public override async ValueTask<JavaServerLaunchResponse> LaunchTaskAsync(Action<(float, string)> action)
        {
            IProgress<(float, string)> progress = new Progress<(float, string)>();
            ((Progress<(float, string)>)progress).ProgressChanged += ProgressChanged!;
            void ProgressChanged(object _, (float, string) e) => action(e);

            Process process = null;
            IEnumerable<string> args = new string[0];
            try
            {
                progress.Report((0.2f, "正在检查Java"));
                if (!File.Exists(LaunchSetting.JvmConfig.JavaPath.FullName))
                {
                    ((Progress<(float, string)>)progress).ProgressChanged -= ProgressChanged;
                    return await Task.FromResult(new JavaServerLaunchResponse(null, LaunchState.Failed, null));
                }
                progress.Report((0.6f, "正在构建启动参数"));
                ArgumentsBuilder = new JavaServerArgumentsBuilder(ServerCore, LaunchSetting);
                args = ArgumentsBuilder.Build();
                Console.WriteLine(string.Join(' '.ToString(), args));
                progress.Report((1f, "正在尝试启动服务器"));
                process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = LaunchSetting.JvmConfig.JavaPath.FullName,
                        Arguments = string.Join(' '.ToString(), args),
                        UseShellExecute = false,
                        WorkingDirectory = ((LaunchSetting.WorkingFolder == null) ? ServerCore.Directory.FullName : (LaunchSetting.WorkingFolder.Exists ? LaunchSetting.WorkingFolder.FullName : ServerCore.Directory.FullName)),
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    },
                    EnableRaisingEvents = true
                };
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                return new JavaServerLaunchResponse(process, LaunchState.Succeess, args)
                {
                    RunTime = stopWatch
                };
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(OperationCanceledException))
                {
                    return await Task.FromResult(new JavaServerLaunchResponse(process, LaunchState.Cancelled, args));
                }
                return await Task.FromResult(new JavaServerLaunchResponse(process, LaunchState.Failed, args, ex));
            }
        }

        public async Task<JavaServerLaunchResponse> LaunchTaskAsync()
        {
            Process process = null;
            IEnumerable<string> args = new string[0];
            try
            {
                if (!File.Exists(LaunchSetting.JvmConfig.JavaPath.FullName))
                {
                    return await Task.FromResult(new JavaServerLaunchResponse(null, LaunchState.Failed, null));
                }
                ArgumentsBuilder = new JavaServerArgumentsBuilder(ServerCore, LaunchSetting);
                args = ArgumentsBuilder.Build();
                Console.WriteLine(string.Join(' '.ToString(), args));
                process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = LaunchSetting.JvmConfig.JavaPath.FullName,
                        Arguments = string.Join(' '.ToString(), args),
                        UseShellExecute = false,
                        WorkingDirectory = ((LaunchSetting.WorkingFolder == null) ? ServerCore.Directory.FullName : (LaunchSetting.WorkingFolder.Exists ? LaunchSetting.WorkingFolder.FullName : ServerCore.Directory.FullName)),
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    },
                    EnableRaisingEvents = true
                };
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                return new JavaServerLaunchResponse(process, LaunchState.Succeess, args)
                {
                    RunTime = stopWatch
                };
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(OperationCanceledException))
                {
                    return await Task.FromResult(new JavaServerLaunchResponse(process, LaunchState.Cancelled, args));
                }
                return await Task.FromResult(new JavaServerLaunchResponse(process, LaunchState.Failed, args, ex));
            }
        }

        public JavaServerLaunchResponse Launch(Action<(float, string)> action)
        {
            return LaunchTaskAsync(action).GetAwaiter().GetResult();
        }

        public JavaServerLaunchResponse Launch()
        {
            return LaunchTaskAsync().GetAwaiter().GetResult();
        }
    }

    partial class JavaServerLauncher
    {
        public JavaServerLauncher(FileInfo ServerCore, LaunchConfig launchSetting)
        {
            LaunchSetting = launchSetting;
            this.ServerCore = ServerCore;
        }

        public override LaunchConfig LaunchSetting { get; set; }

        public FileInfo ServerCore { get; private set; }

        public override JavaServerArgumentsBuilder ArgumentsBuilder { get; set; }
    }
}