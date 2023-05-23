using MinecraftLaunch.Modules.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftLaunch.Modules.Models.Launch
{
    /// <summary>
    /// 游戏日志解析返回模型类
    /// </summary>
    public class GameLogAnalyseResponse
    {
        public GameLogType LogType { get; set; }

        public string Time { get; set; }

        public string Source { get; set; }

        public string Log { get; set; }
    }
}
