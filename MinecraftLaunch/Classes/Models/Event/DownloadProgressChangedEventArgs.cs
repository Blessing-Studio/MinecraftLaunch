using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftLaunch.Classes.Models.Event {
    public class DownloadProgressChangedEventArgs : EventArgs {
        public double Speed { get; set; }

        public int TotalCount { get; set; }

        public double TotalBytes { get; set; }

        public int FailedCount { get; set; }

        public int CompletedCount { get; set; }

        public double DownloadedBytes { get; set; }
    }
}
