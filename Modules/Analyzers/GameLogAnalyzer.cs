using MinecraftLaunch.Modules.Enum;
using MinecraftLaunch.Modules.Interface;
using MinecraftLaunch.Modules.Models.Launch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MinecraftLaunch.Modules.Analyzers {
    /// <summary>
    /// 游戏日志分析器
    /// </summary>
    public partial class GameLogAnalyzer {
        public static GameLogAnalyseResponse AnalyseAsync(string log) {
            return new GameLogAnalyseResponse() {
                Log = GetLog(log),
                Source = GetSource(log),
                Time = Regex.IsMatch(log, "(20|21|22|23|[0-1]\\d):[0-5]\\d:[0-5]\\d", RegexOptions.Compiled) ? GetLogTime(log) : DateTime.Now.ToString(),
                LogType = GetLogType(log) switch {
                    "FATAL" => GameLogType.Fatal,
                    "ERROR" => GameLogType.Error,
                    "WARN" => GameLogType.Warning,
                    "INFO" => GameLogType.Info,
                    "DEBUG" => GameLogType.Debug,
                    "STACK" => GameLogType.StackTrace,
                    "Exception" => GameLogType.Exception,
                    _ => GameLogType.Unknown
                },
            };
        }

        public static string GetLog(string log) {
            var res = GetTotalPrefix(log);
            var s = log.Split(res);
            return (s.Length >= 2 ? s[1] : log).Trim();
        }

        /// <summary>
        /// 获取日志等级类型
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        public static string GetLogType(string log) {
            //是否是堆栈信息
            if (Regex.IsMatch(log, "(at .*)", RegexOptions.Compiled))
                return "STACK";

            //是否是异常信息
            if (Regex.IsMatch(log, "(?m)^.*?Exception.*", RegexOptions.Compiled))
                return "Exception";

            return Regex.Match(log, "FATAL|ERROR|WARN|INFO|DEBUG", RegexOptions.Compiled).Value;
        }

        /// <summary>
        /// 获取日志源
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        public static string GetSource(string log) {
            var content = Regex.Match(log, $"[\\w\\W\\s]{{2,}}/(FATAL|ERROR|WARN|INFO|DEBUG)", RegexOptions.Compiled).Value.Split('/').FirstOrDefault();
            return content?.Replace($"{Regex.Match(log, $"\\[(20|21|22|23|[0-1]\\d):[0-5]\\d:[0-5]\\d\\]").Value} [", string.Empty)!;
        }

        /// <summary>
        /// 获取日志所有前缀
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        public static string GetTotalPrefix(string log) =>
            Regex.Match(log, $"\\[(20|21|22|23|[0-1]\\d):[0-5]\\d:[0-5]\\d\\] \\[[\\w\\W\\s]{{2,}}/(FATAL|ERROR|WARN|INFO|DEBUG)\\]", RegexOptions.Compiled).Value;

        /// <summary>
        /// 获取日志时间
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        public static string GetLogTime(string log) =>
            Regex.Match(log, "(20|21|22|23|[0-1]\\d):[0-5]\\d:[0-5]\\d", RegexOptions.Compiled).Value;
    }
}
