using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftLaunch.Modules.Enum
{
    public enum GameLogType
    {
        Error = 1,
        Info,
        Debug,
        Fatal,
        Warning,
        Exception,
        StackTrace,
        Unknown
    }
}
