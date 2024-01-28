using System.Runtime.Serialization;

namespace MinecraftLaunch.Classes.Models.Exceptions {
    public class ErrorParsingLibraryException(string message) : Exception(message);
}
