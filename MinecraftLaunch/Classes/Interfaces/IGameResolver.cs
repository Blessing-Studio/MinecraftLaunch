using MinecraftLaunch.Classes.Models.Game;

namespace MinecraftLaunch.Classes.Interfaces;

public interface IGameResolver {
    DirectoryInfo Root {  get; }

    GameEntry GetGameEntity(string id);

    IEnumerable<GameEntry> GetGameEntitys();
}