using MinecraftLaunch.Classes.Interfaces;
using MinecraftLaunch.Classes.Models.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftLaunch.Components.Installer {
    public class ForgeInstaller : IInstaller {
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
