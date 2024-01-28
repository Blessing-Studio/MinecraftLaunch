namespace MinecraftLaunch.Classes.Models.Exceptions;

public sealed class ErrorParsingLibraryException(string message) : Exception(message);