using BenchmarkDotNet.Running;
using BenchmarkDotNet.Attributes;
using MinecraftLaunch.Components.Checker;
using MinecraftLaunch.Components.Resolver;

var summary = BenchmarkRunner.Run<BenchmarkClass>();

[MemoryDiagnoser]
public class BenchmarkClass {
    private readonly ResourceChecker _checker;
    private readonly GameResolver _gameResolver = new("D:\\GamePackages\\.minecraft");

    public BenchmarkClass() {
        _checker = new(_gameResolver.GetGameEntity("Life in the village"));
    }

    //[Benchmark]
    //public Task<bool> RunTask() {
    //    return _checker.CheckAsync1();
    //}

    [Benchmark]
    public ValueTask<bool> RunValueTask() {
        return _checker.CheckAsync();
    }
}