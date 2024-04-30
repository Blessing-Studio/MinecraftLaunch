using MinecraftLaunch.Classes.Enums;
using System.Text.RegularExpressions;
using MinecraftLaunch.Classes.Models.Game;

namespace MinecraftLaunch.Components.Resolver;

/// <summary>
/// 游戏日志解析器
/// </summary>
public sealed class GameLogResolver {
    public GameLogEntry Resolve(string log) {
        return new GameLogEntry {
            Log = GetLog(log),
            Source = GetSource(log),
            Time = Regex.IsMatch(log, "(20|21|22|23|[0-1]\\d):[0-5]\\d:[0-5]\\d", RegexOptions.Compiled) ? GetLogTime(log) : DateTime.Now.ToString(),
            LogType = GetLogType(log) switch {
                "FATAL" => LogType.Fatal,
                "ERROR" => LogType.Error,
                "WARN" => LogType.Warning,
                "INFO" => LogType.Info,
                "DEBUG" => LogType.Debug,
                "STACK" => LogType.StackTrace,
                "Exception" => LogType.Exception,
                _ => LogType.Unknown
            },
        };
    }

    public string GetLog(string log) {
        var res = GetTotalPrefix(log);
        var s = log.Split(res);
        return (s.Length >= 2 ? s[1] : log).Trim();
    }

    /// <summary>
    /// 获取日志等级类型
    /// </summary>
    /// <param name="log"></param>
    /// <returns></returns>
    public string GetLogType(string log) {
        //是否是堆栈信息
        if (Regex.IsMatch(log, "(at .*)", RegexOptions.Compiled)) {
            return "STACK";
        }

        //是否是异常信息
        if (Regex.IsMatch(log, "(?m)^.*?Exception.*", RegexOptions.Compiled)) {
            return "Exception";
        }

        return Regex.Match(log, "FATAL|ERROR|WARN|INFO|DEBUG", RegexOptions.Compiled).Value;
    }

    /// <summary>
    /// 获取日志源
    /// </summary>
    /// <param name="log"></param>
    /// <returns></returns>
    public string GetSource(string log) {
        var content = Regex.Match(log, $"[\\w\\W\\s]{{2,}}/(FATAL|ERROR|WARN|INFO|DEBUG)", RegexOptions.Compiled)
            .Value.Split('/')
            .FirstOrDefault();

        return content?.Replace($"{Regex.Match(log,
            $"\\[(20|21|22|23|[0-1]\\d):[0-5]\\d:[0-5]\\d\\]").Value} [",
            string.Empty)!;
    }

    /// <summary>
    /// 获取日志时间
    /// </summary>
    /// <param name="log"></param>
    /// <returns></returns>
    public string GetLogTime(string log) {
        return Regex.Match(log, "(20|21|22|23|[0-1]\\d):[0-5]\\d:[0-5]\\d",
            RegexOptions.Compiled).Value;
    }

    /// <summary>
    /// 获取日志所有前缀
    /// </summary>
    /// <param name="log"></param>
    /// <returns></returns>
    public string GetTotalPrefix(string log) {
        return Regex.Match(log,
            $"\\[(20|21|22|23|[0-1]\\d):[0-5]\\d:[0-5]\\d\\] \\[[\\w\\W\\s]{{2,}}/(FATAL|ERROR|WARN|INFO|DEBUG)\\]",
            RegexOptions.Compiled).Value;
    }
}