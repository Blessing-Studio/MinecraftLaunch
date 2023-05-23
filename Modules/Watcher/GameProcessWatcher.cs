using MinecraftLaunch.Modules.Analyzers;
using MinecraftLaunch.Modules.Interface;
using MinecraftLaunch.Modules.Models.Launch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftLaunch.Modules.Watcher
{
    public class GameProcessWatcher : IWatcher {   
        public int Port { get; protected set; }
        
        public MinecraftLaunchResponse Response { get; protected set; }

        public event EventHandler<AnalysisLogArgs>? AnalysisGameLogOutPut;

        public void StartWatch() {
            Response.ProcessOutput += OnProcessOutput;

            void OnProcessOutput(object? sender, IProcessOutput e) {
                var log = GameLogAnalyzer.AnalyseAsync(e.Raw);

                AnalysisGameLogOutPut?.Invoke(sender, new()
                {
                    LogInfo = log,
                });
            }
        }

        public GameProcessWatcher(MinecraftLaunchResponse response) {
            Response = response;
        }
    }

    public class AnalysisLogArgs
    { 
        public GameLogAnalyseResponse? LogInfo { get; init; }
    }
}
