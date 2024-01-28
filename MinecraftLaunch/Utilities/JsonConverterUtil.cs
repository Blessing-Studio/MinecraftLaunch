using MinecraftLaunch.Components.Converter;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace MinecraftLaunch.Utilities;

public static class JsonConverterUtil {
    public static JsonSerializerOptions DefaultJsonOptions => Get();

    private static JsonSerializerOptions Get() {
        var options = new JsonSerializerOptions {
            MaxDepth = 100,
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

        options.Converters.Add(new AccountJsonConverter());
        options.Converters.Add(new PlatformEnumToStringJsonConverter());

        return options;
    }
}