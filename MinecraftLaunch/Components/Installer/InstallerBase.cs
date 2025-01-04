using MinecraftLaunch.Classes.Interfaces;
using MinecraftLaunch.Classes.Models.Event;
using MinecraftLaunch.Classes.Models.Game;

namespace MinecraftLaunch.Components.Installer;

public abstract class InstallerBase : IInstaller {

    public event EventHandler<EventArgs> Completed;

    public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

    public abstract GameEntry InheritedFrom { get; set; }
    public virtual Func<double, double> CalculateExpression { get; set; }

    public abstract Task<bool> InstallAsync(CancellationToken cancellation = default);

    public void ReportCompleted() {
        Completed?.Invoke(this, EventArgs.Empty);
    }

    internal virtual void ReportProgress(double progress, string progressStatus, TaskStatus status, double speed = 0d) {
        ProgressChanged?.Invoke(this, new(status, CalculateExpression is null ? progress : CalculateExpression.Invoke(progress), progressStatus, speed));
    }
}