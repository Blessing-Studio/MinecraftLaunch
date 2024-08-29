using System.Text.Json;
using MinecraftLaunch.Utilities;
using MinecraftLaunch.Extensions;
using MinecraftLaunch.Classes.Models.Game;

namespace MinecraftLaunch.Components.Resolver.Arguments;

/// <summary>
/// Jvm 虚拟机参数解析器
/// </summary>
internal sealed class JvmArgumentResolver {
    public static IEnumerable<string> Resolve(GameJsonEntry gameJsonEntry) {
        var jvm = new List<string>();

        if (gameJsonEntry.Arguments.Jvm is null) {
            yield return "-Djava.library.path=${natives_directory}";
            yield return "-Dminecraft.launcher.brand=${launcher_name}";
            yield return "-Dminecraft.launcher.version=${launcher_version}";
            yield return "-cp ${classpath}";

            yield break;
        }

        foreach (var arg in gameJsonEntry.Arguments.Jvm) {
            if (arg.ValueKind is JsonValueKind.String) {
                var argValue = arg.GetString().Trim();

                if (argValue.Contains(' ')) {
                    jvm.AddRange(argValue.Split(' '));
                } else {
                    jvm.Add(argValue);
                }
            }
        }

        foreach (var arg in jvm.GroupArguments()) {
            yield return arg;
        }

        //有些沟槽带加载器的版本的 Json 里可能没有 -cp 键，加一个判断以防启动失败
        if (!jvm.Contains("-cp")) {
            yield return "-cp ${classpath}";
        }
    }

    /// <summary>
    /// 获取虚拟机环境参数
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<string> GetEnvironmentJVMArguments() {
        switch (EnvironmentUtil.GetPlatformName()) {
            case "windows":
                yield return "-XX:HeapDumpPath=MojangTricksIntelDriversForPerformance_javaw.exe_minecraft.exe.heapdump";
                if (System.Environment.OSVersion.Version.Major == 10) {
                    yield return "-Dos.name=\"Windows 10\"";
                    yield return "-Dos.version=10.0";
                }
                break;
            case "osx":
                yield return "-XstartOnFirstThread";
                break;
        }

        if (EnvironmentUtil.Arch == "32")
            yield return "-Xss1M";
    }
}