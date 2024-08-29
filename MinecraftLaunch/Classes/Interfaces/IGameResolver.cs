using MinecraftLaunch.Classes.Models.Game;

namespace MinecraftLaunch.Classes.Interfaces;

/// <summary>
/// 游戏实例解析器接口
/// </summary>
public interface IGameResolver {

    /// <summary>
    /// 根目录
    /// </summary>
    DirectoryInfo Root { get; }

    /// <summary>
    /// 根据 Id 获取指定的 <see cref="GameEntry"/> 游戏实例
    /// </summary>
    GameEntry GetGameEntity(string id);

    /// <summary>
    /// 获取目录下所有的 <see cref="GameEntry"/> 游戏实例
    /// </summary>
    IEnumerable<GameEntry> GetGameEntitys();
}