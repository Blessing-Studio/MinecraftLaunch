using MinecraftLaunch.Classes.Interfaces;
using MinecraftLaunch.Classes.Models.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftLaunch.Components.Installer {
    public abstract class InstallerBase : IInstaller {
        public event EventHandler<EventArgs> Completed;

        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

        public abstract ValueTask<bool> InstallAsync();

        public virtual void ReportProgress(double progress, string progressStatus, TaskStatus status) {
            ProgressChanged?.Invoke(this, new(status, progress, progressStatus));
        }
    }
}
