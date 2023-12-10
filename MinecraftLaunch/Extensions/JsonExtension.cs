using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;

namespace MinecraftLaunch.Extensions {
    public static class JsonExtension {
        public static string AsJson(this object obj) {
            return JsonSerializer.Serialize(obj);
        }

        public static T AsJsonEntry<T>(this string json) {
            return JsonSerializer.Deserialize<T>(json);
        }

        public static int GetInt32(this JsonNode node) {
            return node.GetValue<int>();
        }

        public static int GetInt32(this JsonNode node, string name) {
            return node[name].GetValue<int>();
        }

        public static bool GetBool(this JsonNode node) {
            return node.GetValue<bool>();
        }

        public static bool GetBool(this JsonNode node, string name) {
            return node[name].GetValue<bool>();
        }

        public static string GetString(this JsonNode node) {
            return node.GetValue<string>();
        }

        public static string GetString(this JsonNode node, string name) {
            return node[name].GetValue<string>();
        }

        public static JsonArray GetEnumerable(this JsonNode node) {
            return node.AsArray();
        }

        public static JsonArray GetEnumerable(this JsonNode node, string name) {
            return node[name].AsArray();
        }

        public static IEnumerable<T> GetEnumerable<T>(this JsonNode node) {
            return node.AsArray()
                .Select(x => x.GetValue<T>());
        }

        public static IEnumerable<T> GetEnumerable<T>(this JsonNode node, string name) {
            return node[name]
                .AsArray()
                .Select(x => x.GetValue<T>());
        }

        public static IEnumerable<T> GetEnumerable<T>(this JsonNode node, string name, string elementName) {
            return node[name]
                .AsArray()
                .Select(x => x[elementName].GetValue<T>());
        }
    }
}
