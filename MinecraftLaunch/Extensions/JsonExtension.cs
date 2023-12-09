using System.Text.Json;
using System.Text.Json.Nodes;

namespace MinecraftLaunch.Extensions {
    public static class JsonExtension {
        public static string AsJson(this object obj) {
            return JsonSerializer.Serialize(obj);
        }

        public static T AsJsonEntry<T>(this string json) {
            return JsonSerializer.Deserialize<T>(json);
        }

        public static int GetInt32(this JsonNode node, string name) {
            return node[name].GetValue<int>();
        }

        public static bool GetBool(this JsonNode node, string name) {
            return node[name].GetValue<bool>();
        }

        public static string GetString(this JsonNode node, string name) {
            return node[name].GetValue<string>();
        }
    }
}
