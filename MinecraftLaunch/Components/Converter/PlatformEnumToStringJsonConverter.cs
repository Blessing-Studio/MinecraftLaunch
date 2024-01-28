using System.Text.Json;
using MinecraftLaunch.Classes.Enums;
using System.Text.Json.Serialization;

namespace MinecraftLaunch.Components.Converter;

public sealed class PlatformEnumToStringJsonConverter : JsonConverter<Platform> {
    public override Platform Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        var platform = reader.GetString();
        return platform switch {
            "osx" => Platform.osx,
            "linux" => Platform.linux,
            "windows" => Platform.windows,
            _ => Platform.windows,
        };
    }

    public override void Write(Utf8JsonWriter writer, Platform value, JsonSerializerOptions options) {
        writer.WriteStringValue(value.ToString());
    }
}