namespace MinecraftLaunch.Classes.Interfaces;

public interface IResolver;

public interface IResolver<out T> : IResolver {
    T Resolve(string str);
}

public interface IResolver<out TReturn, in TParameter> : IResolver {
    TReturn Resolve(TParameter parameter);
}