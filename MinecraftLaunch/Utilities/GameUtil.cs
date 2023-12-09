using MinecraftLaunch.Classes.Enums;
using MinecraftLaunch.Classes.Models.Game;

namespace MinecraftLaunch.Utilities {
    public static class GameUtil {
        public static LoaderType GetGameLoaderType(this GameJsonEntry entity) {
            return entity.MainClass switch {
                "net.minecraft.launchwrapper.Launch" => LoaderType.Forge,
                "net.fabricmc.loader.impl.launch.knot.KnotClient" => LoaderType.Fabric,
                _ => LoaderType.Vanilla
            };
        }
    }
}
