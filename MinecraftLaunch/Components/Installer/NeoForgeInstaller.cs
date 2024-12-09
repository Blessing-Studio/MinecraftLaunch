using MinecraftLaunch.Classes.Models.Game;

namespace MinecraftLaunch.Components.Installer;

public sealed class NeoForgeInstaller : InstallerBase {
    public override GameEntry InheritedFrom => throw new NotImplementedException();

    public override Task<bool> InstallAsync(CancellationToken cancellation = default) {
        throw new NotImplementedException();
    }
}