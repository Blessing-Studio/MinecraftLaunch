using MinecraftLaunch.Classes.Models.Game;

namespace MinecraftLaunch.Classes.Interfaces {
    public interface IGameResolver {
        GameEntry GetGameEntity(string id);

        IEnumerable<GameEntry> GetGameEntitys();
    }
}
