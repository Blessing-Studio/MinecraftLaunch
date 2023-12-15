using MinecraftLaunch.Classes.Interfaces;
using MinecraftLaunch.Classes.Models.Event;

namespace MinecraftLaunch.Components.Installer {
    public class QuiltInstaller : IInstaller {
        public event EventHandler<EventArgs> Completed;

        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

        public ValueTask<bool> InstallAsync() {
            throw new NotImplementedException();
        }

        public void ReportProgress(double progress, string progressStatus, TaskStatus status) {
            throw new NotImplementedException();
        }
    }
}
