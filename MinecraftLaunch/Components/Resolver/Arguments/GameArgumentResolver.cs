using MinecraftLaunch.Extensions;
using MinecraftLaunch.Classes.Models.Game;
using System.Collections.Immutable;
using System.Text.Json;

namespace MinecraftLaunch.Components.Resolver.Arguments;

/// <summary>
/// 游戏参数解析器
/// </summary>
internal sealed class GameArgumentResolver {
    /// <summary>
    /// 解析参数
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<string> Resolve(GameJsonEntry gameJsonEntry) {
        if (!string.IsNullOrEmpty(gameJsonEntry.MinecraftArguments)) {
            foreach (var arg in gameJsonEntry.MinecraftArguments.Split(' ').GroupArguments()) {
                yield return arg;
            }
        }

        if (gameJsonEntry?.Arguments?.Game is null) {
            yield break;
        }

        var game = gameJsonEntry.Arguments.Game
            .Where(x => x.ValueKind is JsonValueKind.String)
            .Select(x => x.GetString().ToPath())
            .GroupArguments();

        foreach (var item in game.ToImmutableArray()) {
            yield return item;
        }
    }
}