using System.Collections.Generic;
using System.IO;
using MinecraftLaunch.Modules.Interface;
using MinecraftLaunch.Modules.Models.Launch;
using MinecraftLaunch.Modules.Utilities;

namespace MinecraftLaunch.Modules.ArgumentsBuilders;

public class JavaServerArgumentsBuilder : IArgumentsBuilder {
    public LaunchConfig? LaunchConfig { get; private set; }

    public FileInfo ServerCore { get; private set; }

    public IEnumerable<string?> Build() {
        return GetFrontArguments();
    }

    public IEnumerable<string?> GetBehindArguments() {
        return new string[6] { "-${SevrerOrClient}", "-Xmx${Max}M", "-Xms${Mini}M", "-jar", "${ServerCore}", "nogui" };
    }

    public IEnumerable<string?> GetFrontArguments() {
        IEnumerable<string> args = GetBehindArguments();
        Dictionary<string, string> keyValuePairs = new Dictionary<string, string>
        {
            {
                "${SevrerOrClient}",
                LaunchConfig.IsServer ? "server" : "client"
            },
            {
                "${Max}",
                LaunchConfig.JvmConfig.MaxMemory.ToString()
            },
            {
                "${Mini}",
                LaunchConfig.JvmConfig.MinMemory.ToString()
            },
            { "${ServerCore}", ServerCore.FullName }
        };
        foreach (string item in args) {
            yield return item.Replace(keyValuePairs);
        }
    }

    public JavaServerArgumentsBuilder(FileInfo ServerCore, LaunchConfig? launchConfig) {
        this.ServerCore = ServerCore;
        LaunchConfig = launchConfig;
    }
}
