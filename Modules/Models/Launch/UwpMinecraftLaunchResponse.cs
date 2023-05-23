using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using MinecraftLaunch.Modules.Enum;

namespace MinecraftLaunch.Modules.Models.Launch;

public class UwpMinecraftLaunchResponse : IDisposable
{
	private bool disposedValue;

	public LaunchState State { get; private set; }

	public Process Process { get; private set; }

	public Stopwatch RunTime { get; set; }

	public Exception Exception { get; private set; }

	public void WaitForExit()
	{
		Process?.WaitForExit();
	}

	public async Task WaitForExitAsync()
	{
		await Task.Run(delegate
		{
			Process?.WaitForExit();
		});
	}

	public void Stop()
	{
		Process?.Kill();
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			Process?.Dispose();
			Exception = null;
			disposedValue = true;
		}
	}

	public UwpMinecraftLaunchResponse(Process process, LaunchState state, IEnumerable<string> args)
	{
		Process = process;
		State = state;
		if (state == LaunchState.Succeess)
		{
			Process.Start();
		}
	}

	public UwpMinecraftLaunchResponse(Process process, LaunchState state, IEnumerable<string> args, Exception exception)
	{
		Process = process;
		State = state;
		Exception = exception;
	}
}
