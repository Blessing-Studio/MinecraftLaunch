using MinecraftLaunch.Modules.Interface;
using MinecraftLaunch.Modules.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftLaunch.Modules.Watcher
{
    /// <summary>
    /// 游戏核心监视器
    /// </summary>
    public class GameCoresWatcher : IWatcher
    {
        public event EventHandler<GameCoresChangedArgs>? GameCoresChanged;

        public GameCoresWatcher(GameCoreUtil toolkit) {       
            Toolkit = toolkit;
        }

        public GameCoreUtil Toolkit { get; private set; }

        public void StartWatch() {
            FileSystemWatcher watcher = new(Path.Combine(Toolkit.Root.FullName, "versions"));
            watcher.EnableRaisingEvents = true;

            watcher.Changed += (_, x) => {
                GameCoresChanged?.Invoke(this, new(WatcherChangeTypes.Changed, x.FullPath.IsDirectory() ? x.Name! : string.Empty));
            };

            watcher.Created += (_, x) => {
                GameCoresChanged?.Invoke(this, new(WatcherChangeTypes.Created, x.FullPath.IsDirectory() ? x.Name! : string.Empty));
            };

            watcher.Deleted += (_, x) => {
                GameCoresChanged?.Invoke(this, new(WatcherChangeTypes.Deleted, x.FullPath.IsDirectory() ? x.Name! : string.Empty));
            };

            watcher.Renamed += (_, x) => {
                GameCoresChanged?.Invoke(this, new(WatcherChangeTypes.Renamed, x.FullPath.IsDirectory() ? x.Name! : string.Empty));
            };
        }
    }

    public class GameCoresChangedArgs {
        public GameCoresChangedArgs(WatcherChangeTypes types,string id) {
            ChangeType= types;
            GameCoreId= id;
        }

        public WatcherChangeTypes ChangeType { get; set; }

        public string GameCoreId { get; set; }
    }
}
