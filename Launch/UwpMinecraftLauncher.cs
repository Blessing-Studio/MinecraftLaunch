using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using MinecraftLaunch.Modules.Enum;
using MinecraftLaunch.Modules.Interface;
using MinecraftLaunch.Modules.Models.Launch;

namespace MinecraftLaunch.Launch;

public class UwpMinecraftLauncher : LauncherBase<object, UwpMinecraftLaunchResponse>
{
	public new LaunchConfig LaunchSetting { get; set; }

	public override async ValueTask<UwpMinecraftLaunchResponse> LaunchTaskAsync(Action<(float, string)> action)
	{
		IProgress<(float, string)> progress = new Progress<(float, string)>();
		((Progress<(float, string)>)progress).ProgressChanged += ProgressChanged;
		void ProgressChanged(object _, (float, string) e) => action(e);

        Process process = null;
		IEnumerable<string> args = new string[0];
		try
		{
			progress.Report((0.5f, "正在检查游戏是否安装"));
			using Process checkprocess = new Process
			{
				StartInfo = new ProcessStartInfo("C:\\Windows\\System32\\WindowsPowerShell\\v1.0\\powershell.exe")
				{
					WorkingDirectory = Environment.CurrentDirectory,
					RedirectStandardOutput = true,
					CreateNoWindow = true,
					Arguments = "Get-AppxPackage -Name \"Microsoft.MinecraftUWP\""
				}
			};
			checkprocess.Start();
			if (string.IsNullOrEmpty(checkprocess.StandardOutput.ReadToEnd()))
			{
				((Progress<(float, string)>)progress).ProgressChanged -= ProgressChanged;
				return await Task.FromResult(new UwpMinecraftLaunchResponse(null, LaunchState.Failed, null));
			}
			progress.Report((1f, "正在尝试启动游戏"));
			process = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					UseShellExecute = true,
					FileName = "minecraft:"
				}
			};
			Stopwatch stopWatch = new Stopwatch();
			stopWatch.Start();
			return new UwpMinecraftLaunchResponse(process, LaunchState.Succeess, args)
			{
				RunTime = stopWatch
			};
		}
		catch (Exception ex)
		{
			if (ex.GetType() == typeof(OperationCanceledException))
			{
				return await Task.FromResult(new UwpMinecraftLaunchResponse(process, LaunchState.Cancelled, args));
			}
			return await Task.FromResult(new UwpMinecraftLaunchResponse(process, LaunchState.Failed, args, ex));
		}
	}
}
