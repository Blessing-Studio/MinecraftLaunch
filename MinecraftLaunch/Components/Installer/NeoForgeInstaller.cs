using MinecraftLaunch.Classes.Models.Game;

namespace MinecraftLaunch.Components.Installer;

public sealed class NeoForgeInstaller : InstallerBase {
    public override GameEntry InheritedFrom => throw new NotImplementedException();

    public override ValueTask<bool> InstallAsync() {
        throw new NotImplementedException();
    }
}