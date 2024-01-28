using MinecraftLaunch.Classes.Models.Event;

namespace MinecraftLaunch.Classes.Interfaces;

public interface IInstaller {
    ValueTask<bool> InstallAsync();

    void ReportProgress(double progress, string progressStatus, TaskStatus status);

    event EventHandler<EventArgs> Completed;

    event EventHandler<ProgressChangedEventArgs> ProgressChanged;
}