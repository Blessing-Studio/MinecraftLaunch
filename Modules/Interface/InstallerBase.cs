using System;
using MinecraftLaunch.Events;

namespace MinecraftLaunch.Modules.Interface;

public abstract class InstallerBase<T>
{
	public event EventHandler<ProgressChangedEventArgs>? ProgressChanged;

	public abstract ValueTask<T> InstallAsync();

    internal void InvokeStatusChangedEvent(float progress, string progressdescription)
	{
		this.ProgressChanged?.Invoke(this, new()
		{
			ProgressDescription = progressdescription,
			Progress = progress
		});
	}
}
