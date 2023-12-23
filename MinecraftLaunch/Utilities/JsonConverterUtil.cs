using MinecraftLaunch.Components.Converter;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace MinecraftLaunch.Utilities {
    public class JsonConverterUtil {
        public static JsonSerializerOptions DefaultJsonConverterOptions => Get();

        private static JsonSerializerOptions Get() {
            var options = new JsonSerializerOptions() {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            };

            options.Converters.Add(new AccountJsonConverter());
            options.Converters.Add(new PlatformEnumToStringJsonConverter());

            return options;
        }
    }
}
