using MinecraftLaunch.Modules.Models.Launch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftLaunch.Modules.Models.Download
{
    public class Saves {
        public List<SavesPlayer> SavesPlayers { get; set; }

        public GameCore? RootGameCore { get; set; }

        public string? Id { get; set; }

        public long? LastPlayed { get; set; }

        public long? Time { get; set; }

        public int? GameType { get; set; }

        public byte? HardCore { get; set; }

        public bool HasVillages { get; set; }

        public bool IsRaining { get; set; }

        public bool IsThundering { get; set; }
    }

    public class SavesPlayer {
        public string PlayUuid { get; set; }

        public bool IsSleeping { get; set; }

        public short Health { get; set; }

        public int FoodLevel { get; set; }
    }
}
