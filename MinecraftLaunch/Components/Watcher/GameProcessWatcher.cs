using System.Diagnostics;
using MinecraftLaunch.Classes.Interfaces;
using MinecraftLaunch.Components.Resolver;
using MinecraftLaunch.Classes.Models.Event;

namespace MinecraftLaunch.Components.Watcher;

/// <summary>
/// 游戏进程监视器
/// </summary>
public sealed class GameProcessWatcher : IWatcher, IGameProcessWatcher {
    private readonly GameLogResolver _gameLogResolver;

    public Process Process { get; }
    public IEnumerable<string> Arguments { get; }

    public event EventHandler<ExitedEventArgs> Exited;
    public event EventHandler<LogReceivedEventArgs> OutputLogReceived;

    public GameProcessWatcher(Process process, IEnumerable<string> arguments) {
        Process = process;
        Arguments = arguments;
        _gameLogResolver = new();

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
        if (!string.IsNullOrEmpty(e.Data)) {
            var log = _gameLogResolver.Resolve(e.Data);
            OutputLogReceived?.Invoke(this, new(e.Data, log.Log, log.Time, log.Time, log.LogType));
        }
    }
}