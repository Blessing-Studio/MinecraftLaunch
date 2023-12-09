using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftLaunch.Classes.Models.Event {
    public class ExitedEventArgs(int exitCode) : EventArgs {
        public int ExitCode => exitCode;
    }
}
