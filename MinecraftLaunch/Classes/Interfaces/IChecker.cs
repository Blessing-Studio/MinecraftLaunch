using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftLaunch.Classes.Interfaces {
    /// <summary>
    /// 检查器统一接口
    /// </summary>
    public interface IChecker {
        ValueTask<bool> CheckAsync();
    }
}
