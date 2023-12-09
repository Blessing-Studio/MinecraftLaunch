using MinecraftLaunch.Classes.Interfaces;
using MinecraftLaunch.Classes.Models.Event;

namespace MinecraftLaunch.Components.Installer {
    /// <summary>
    /// 原版核心安装器
    /// </summary>
    public class VanlliaInstaller : IInstaller {
        public event EventHandler<EventArgs> Completed;

        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

        public ValueTask<bool> InstallAsync() {
            throw new NotImplementedException();
        }
    }
}
