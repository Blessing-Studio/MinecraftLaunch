using MinecraftLaunch.Base.EventArgs;
using MinecraftLaunch.Base.Interfaces;
using MinecraftLaunch.Base.Models.Game;

namespace MinecraftLaunch.Components.Installer;

public abstract class InstallerBase : IInstaller {
    public abstract string MinecraftFolder { get; init; }

    public event EventHandler<EventArgs> Completed;
    public event EventHandler<InstallProgressChangedEventArgs> ProgressChanged;

    public abstract Task<MinecraftEntry> InstallAsync(CancellationToken cancellationToken = default);

    public void ReportCompleted() {
        Completed?.Invoke(this, EventArgs.Empty);
    }

    internal virtual void ReportProgress(double progress, string progressStatus, TaskStatus status, double speed = -1d) {
        ProgressChanged?.Invoke(this, new InstallProgressChangedEventArgs {
            Speed = speed,
            Status = status,
            Progress = progress,
            ProgressStatus = progressStatus
        });
    }
}