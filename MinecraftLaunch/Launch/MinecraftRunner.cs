using MinecraftLaunch.Base.Models.Game;
using MinecraftLaunch.Components.Parser;
using MinecraftLaunch.Extensions;

namespace MinecraftLaunch.Launch;

public sealed class MinecraftRunner {
    private readonly MinecraftParser _minecraftParser;

    public LaunchConfig LaunchConfig { get; set; }

    public MinecraftRunner(LaunchConfig launchConfig, MinecraftParser parser) {
        _minecraftParser = parser;
        LaunchConfig = launchConfig;
    }

    public MinecraftProcess Run(string id) {
        var minecraftEntry = _minecraftParser.GetMinecraft(id);
        ArgumentsParser parser = new(minecraftEntry, LaunchConfig);
        if (string.IsNullOrEmpty(LaunchConfig.NativesFolder))
            minecraftEntry.ExtractNatives(parser.GetNatives());

        return new MinecraftProcess(LaunchConfig, minecraftEntry, parser.Parse());
    }

    public MinecraftProcess Run(MinecraftEntry minecraft) => Run(minecraft.Id);

    public async Task<MinecraftProcess> RunAsync(string id, CancellationToken cancellationToken = default) {
        var minecraftEntry = _minecraftParser.GetMinecraft(id);
        ArgumentsParser parser = new(minecraftEntry, LaunchConfig);
        if (string.IsNullOrEmpty(LaunchConfig.NativesFolder))
            await minecraftEntry.ExtractNativesAsync(parser.GetNatives(), cancellationToken);

        return new MinecraftProcess(LaunchConfig, minecraftEntry, parser.Parse());
    }

    public Task<MinecraftProcess> RunAsync(MinecraftEntry minecraft, CancellationToken cancellationToken = default) => RunAsync(minecraft.Id, cancellationToken);
}