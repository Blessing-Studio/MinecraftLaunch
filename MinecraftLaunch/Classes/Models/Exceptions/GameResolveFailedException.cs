namespace MinecraftLaunch.Classes.Models.Exceptions;

public sealed class GameResolveFailedException(string message) : Exception(message);