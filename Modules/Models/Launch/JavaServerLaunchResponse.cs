using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using MinecraftLaunch.Events;
using MinecraftLaunch.Modules.Enum;
using MinecraftLaunch.Modules.Interface;

namespace MinecraftLaunch.Modules.Models.Launch;

public class JavaServerLaunchResponse : IDisposable
{
	private List<string> Output = new List<string>();

	private string Cache = string.Empty;

	private bool disposedValue;

	public LaunchState State { get; private set; }

	public IEnumerable<string> Arguemnts { get; private set; }

	public Process Process { get; private set; }

	public Stopwatch RunTime { get; set; }

	public Exception Exception { get; private set; }

	public event EventHandler<ExitedArgs> Exited;

	public event EventHandler<IProcessOutput> ProcessOutput;

	public void WaitForExit()
	{
		Process?.WaitForExit();
	}

	public void Refresh()
	{
		Process?.Refresh();
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
		if (disposedValue)
		{
			return;
		}
		Process?.Dispose();
		Arguemnts = null;
		Output = null;
		Exception = null;
		if (this.Exited != null)
		{
			Delegate[] invocationList = this.Exited.GetInvocationList();
			foreach (Delegate @delegate in invocationList)
			{
				Exited -= (EventHandler<ExitedArgs>)@delegate;
			}
		}
		if (this.ProcessOutput != null)
		{
			Delegate[] invocationList = this.ProcessOutput.GetInvocationList();
			foreach (Delegate delegate2 in invocationList)
			{
				ProcessOutput -= (EventHandler<IProcessOutput>)delegate2;
			}
		}
		disposedValue = true;
	}

	private void AddOutput(string text)
	{
		if (!string.IsNullOrEmpty(text))
		{
			Cache = text;
			Output.Add(text);
			this.ProcessOutput?.Invoke(this, new BaseProcessOutput(Cache));
			Cache = string.Empty;
		}
	}

	public JavaServerLaunchResponse(Process process, LaunchState state, IEnumerable<string> args)
	{
		Process = process;
		State = state;
		Arguemnts = args;
		if (state == LaunchState.Succeess)
		{
			Process.OutputDataReceived += delegate(object _, DataReceivedEventArgs e)
			{
				AddOutput(e.Data);
			};
			Process.ErrorDataReceived += delegate(object _, DataReceivedEventArgs e)
			{
				AddOutput(e.Data);
			};
			Process.Exited += delegate
			{
				RunTime.Stop();
				this.Exited?.Invoke(this, new ExitedArgs
				{
					Crashed = (Process.ExitCode != 0),
					ExitCode = Process.ExitCode,
					RunTime = RunTime,
					Outputs = Output
				});
			};
			Process.Start();
			Process.BeginOutputReadLine();
			Process.BeginErrorReadLine();
		}
	}

	public JavaServerLaunchResponse(Process process, LaunchState state, IEnumerable<string> args, Exception exception)
	{
		Process = process;
		State = state;
		Arguemnts = args;
		Exception = exception;
	}
}
