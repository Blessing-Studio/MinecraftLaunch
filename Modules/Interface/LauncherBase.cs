using System;
using System.Threading.Tasks;
using MinecraftLaunch.Modules.Models.Launch;

namespace MinecraftLaunch.Modules.Interface;

public abstract class LauncherBase<T, T2>
{
	public virtual LaunchConfig LaunchSetting { get; set; }

	public virtual T ArgumentsBuilder { get; set; }

	public virtual ValueTask<T2> LaunchTaskAsync(string id, Action<(float, string)> action)
	{
		throw new Exception();
	}

	public virtual ValueTask<T2> LaunchTaskAsync(Action<(float, string)> action)
	{
		throw new Exception();
	}
}
