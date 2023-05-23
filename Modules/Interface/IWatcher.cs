using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftLaunch.Modules.Interface
{
    /// <summary>
    /// 监视器通用接口
    /// </summary>
    public interface IWatcher {
        /// <summary>
        /// 开始监视
        /// </summary>
       void StartWatch();
    }
}
