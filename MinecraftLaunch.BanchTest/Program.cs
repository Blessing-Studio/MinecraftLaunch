using BenchmarkDotNet.Running;
using BenchmarkDotNet.Attributes;
using MinecraftLaunch.Components.Checker;
using MinecraftLaunch.Components.Resolver;
using MinecraftLaunch.Extensions;
using MinecraftLaunch.Classes.Interfaces;
using System.Threading.Tasks.Dataflow;
using MinecraftLaunch.Utilities;
using System.Diagnostics;

//var summary = BenchmarkRunner.Run<BenchmarkClass>();

await new BenchmarkClass().RunTaskParallel();
Console.ReadKey();

[MemoryDiagnoser]
public class BenchmarkClass {
    private readonly ResourceChecker _checker;
    private readonly GameResolver _gameResolver = new("C:\\Users\\w\\Desktop\\temp\\.minecraft");

    public BenchmarkClass() {
        _checker = new(_gameResolver.GetGameEntity("1.16.5"));
        _ = _checker.CheckAsync();
    }
}

public static class Downloader {

}