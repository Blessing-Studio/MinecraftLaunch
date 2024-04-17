using System;
using System.Diagnostics;
using MinecraftLaunch.Classes.Interfaces;
using MinecraftLaunch.Classes.Models.Event;

namespace MinecraftLaunch.Components.Watcher;

/// <summary>
/// 游戏进程监视器
/// </summary>
public class GameProcessWatcher : IWatcher, IGameProcessWatcher {
    public Process Process { get; }

    public IEnumerable<string> Arguments { get; }

    public event EventHandler<ExitedEventArgs> Exited;

    public event EventHandler<LogReceivedEventArgs> OutputLogReceived;

    public GameProcessWatcher(Process process, IEnumerable<string> arguments) {
        Process = process;
        Arguments = arguments;
        Start();
    }

    public void Start() {
        Process.Exited += OnExited;
        Process.ErrorDataReceived += OnOutputDataReceived;
        Process.OutputDataReceived += OnOutputDataReceived;

        Process.Start();
        Process.BeginErrorReadLine();
        Process.BeginOutputReadLine();
    }

    private void OnExited(object sender, EventArgs e) {
        using (Process) {
            Exited?.Invoke(this, new(Process.ExitCode));
        }
    }

    private void OnOutputDataReceived(object sender, DataReceivedEventArgs e) {
        if(!string.IsNullOrEmpty(e.Data)) {
            OutputLogReceived?.Invoke(this, new(e.Data));
        }
    }
}