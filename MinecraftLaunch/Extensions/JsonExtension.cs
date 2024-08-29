using MinecraftLaunch.Utilities;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace MinecraftLaunch.Extensions;

public static class JsonExtension {

    public static string Serialize(this object value, Type inputType, JsonSerializerContext context) {
        return JsonSerializer.Serialize(value, inputType, context);
    }

    public static T Deserialize<T>(this string json, JsonTypeInfo<T> jsonType) {
        return JsonSerializer.Deserialize(json, jsonType);
    }

    public static string AsJson(this object obj) {
        return JsonSerializer.Serialize(obj, JsonConverterUtil.DefaultJsonOptions);
    }

    public static T AsJsonEntry<T>(this string json) {
        return JsonSerializer.Deserialize<T>(json, JsonConverterUtil.DefaultJsonOptions);
    }

    public static JsonNode AsNode(this string json) {
        return JsonNode.Parse(json);
    }

    public static JsonNode Select(this JsonNode node, string name) {
        return node[name];
    }

    public static int GetInt32(this JsonNode node) {
        return node.GetValue<int>();
    }

    public static int GetInt32(this JsonNode node, string name) {
        return node.Select(name).GetValue<int>();
    }

    public static bool GetBool(this JsonNode node) {
        return node.GetValue<bool>();
    }

    public static bool GetBool(this JsonNode node, string name) {
        return node.Select(name).GetValue<bool>();
    }

    public static string GetString(this JsonNode node) {
        return node?.GetValue<string>();
    }

    public static string GetString(this JsonNode node, string name) {
        return node.Select(name)?.GetValue<string>();
    }

    public static JsonArray GetEnumerable(this JsonNode node) {
        return node.AsArray();
    }

    public static JsonArray GetEnumerable(this JsonNode node, string name) {
        return node.Select(name).AsArray();
    }

    public static IEnumerable<T> GetEnumerable<T>(this JsonNode node) {
        return node.AsArray()
            .Select(x => x.GetValue<T>());
    }

    public static IEnumerable<T> GetEnumerable<T>(this JsonNode node, string name) {
        return node.Select(name)
            .AsArray()
            .Select(x => x.GetValue<T>());
    }

    public static IEnumerable<T> GetEnumerable<T>(this JsonNode node, string name, string elementName) {
        return node.Select(name)
            .AsArray()
            .Select(x => x.Select(elementName).GetValue<T>());
    }

    public static JsonNode SetString(this JsonNode node, string name, string value) {
        node[name] = value;
        return node;
    }
}