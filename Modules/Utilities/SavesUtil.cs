using MinecraftLaunch.Modules.Interface;
using MinecraftLaunch.Modules.Models.Download;
using MinecraftLaunch.Modules.Models.Launch;
using NbtLib;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftLaunch.Modules.Utilities
{
    /// <summary>
    /// 存档操作工具箱
    /// </summary>
    public partial class SavesUtil {   
        public async ValueTask<ImmutableArray<Saves>> LoadAllAsync(GameCore core) {       
            List<Saves> res = new();
            var saves = Path.Combine(core.Root.FullName, "versions", core.Id, "saves");
            
            if (saves.IsDirectory()) { 
                foreach (var i in saves.FindAllDirectory().AsParallel()) {
                    List<SavesPlayer> players = new();
                    var mainFile = Path.Combine(i.FullName, "level.dat");
                    var playerData = Path.Combine(i.FullName, "playerdata");
                    var currentsaves = new Saves();

                    if (File.Exists(mainFile)) {                    
                        var tags = NbtUtil.Load(mainFile);
                        var tag = (tags["Data"] as NbtCompoundTag)!;
                        
                        var saveName = ((NbtStringTag)tag["LevelName"]).Payload;
                        var lastPlayed = ((NbtLongTag)tag["LastPlayed"]).Payload;
                        var time = ((NbtLongTag)tag["Time"]).Payload;
                        var gameType = ((NbtIntTag)tag["GameType"]).Payload;
                        var hardCore = ((NbtByteTag)tag["hardcore"]).Payload;
                        var hasVillages = ((NbtByteTag)tag["MapFeatures"]).Payload;
                        var isRaining = ((NbtByteTag)tag["raining"]).Payload;
                        var thundering = ((NbtByteTag)tag["thundering"]).Payload;

                        currentsaves = new()
                        {
                            Id = saveName,
                            LastPlayed = lastPlayed,
                            GameType = gameType,
                            HardCore = Convert.ToByte(hardCore),
                            HasVillages = hasVillages is 1,
                            IsRaining = isRaining is 1,
                            IsThundering = thundering is 1,
                            Time = time,
                            RootGameCore = core
                        };
                    }

                    if (playerData.IsDirectory()) {
                        foreach (var p in playerData.FindAllFile().AsParallel()) {
                            var tags = NbtUtil.Load(p.FullName);

                            players.Add(new()
                            {
                                PlayUuid = Path.GetFileNameWithoutExtension(p.Name),
                                IsSleeping = ((NbtByteTag)tags["Sleeping"]).Payload is 1,
                                FoodLevel = ((NbtIntTag)tags["foodLevel"]).Payload,
                                Health = ((NbtShortTag)tags["Health"]).Payload
                            });;
                        }

                        currentsaves.SavesPlayers = players;
                    }

                    res.Add(currentsaves);
                }
            }

            return res.ToImmutableArray();
        }
    }

    partial class SavesUtil {   
        public SavesUtil(GameCoreUtil core) {       
            Toolkit = core;
        }
    }

    partial class SavesUtil {   
        public GameCoreUtil Toolkit { get; set; }
    }
}
