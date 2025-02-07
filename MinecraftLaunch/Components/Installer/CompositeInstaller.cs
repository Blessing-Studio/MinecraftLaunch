using MinecraftLaunch.Base.Models.Game;

namespace MinecraftLaunch.Components.Installer;

public sealed class CompositeInstaller : InstallerBase {
    public override string MinecraftFolder { get => throw new NotImplementedException(); init => throw new NotImplementedException(); }

    public override Task<MinecraftEntry> InstallAsync(CancellationToken cancellationToken = default) {
        throw new NotImplementedException();
    }
}