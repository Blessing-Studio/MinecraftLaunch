using MinecraftLaunch.Classes.Models.Event;

namespace MinecraftLaunch.Classes.Interfaces;

public interface IInstaller {
    ValueTask<bool> InstallAsync();

    event EventHandler<EventArgs> Completed;

    event EventHandler<ProgressChangedEventArgs> ProgressChanged;
}