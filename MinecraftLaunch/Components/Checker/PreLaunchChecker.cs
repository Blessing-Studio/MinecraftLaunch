using MinecraftLaunch.Classes.Interfaces;
using MinecraftLaunch.Classes.Models.Game;

namespace MinecraftLaunch.Components.Checker;

/// <summary>
/// 预启动检查器
/// </summary>
/// <param name="entry"></param>
public sealed class PreLaunchChecker(GameEntry entry) : IChecker {
    public bool IsCheckResource { get; set; }

    public bool IsCheckAccount { get; set; }
    public GameEntry Entry => entry;

    public ValueTask<bool> CheckAsync() {
        /*
         *
         */
        throw new NotImplementedException();
    }
}