﻿using System.Text.RegularExpressions;
using MinecraftLaunch.Classes.Interfaces;

namespace MinecraftLaunch.Components.Resolver;

/// <summary>
/// Toml 文件解析器
/// </summary>
public sealed class TomlResolver() {
    private readonly char[] _separator = ['\r', '\n'];
    private readonly Dictionary<string, string> _data = new();
    private readonly Regex _regex = new(@"(?<=(" + "\"" + "))[.\\s\\S]*?(?=(" + "\"" + "))");

    public TomlResolver(string content) : this() {
        Parse(content);
    }

    public string this[string key] => GetString(key);

    public string GetString(string key) {
        return _data.GetValueOrDefault(key);
    }

    private void Parse(string content) {
        var lines = content.Split(_separator, StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines) {
            if (line.StartsWith("#")) {
                continue;
            }

            var parts = line.Split(['='], 2);
            if (parts.Length == 2) {
                var key = parts[0].Trim();
                var value = _regex.Match(parts[1]).Value;
                    
                _data[key] = value;
            }
        }
    }
}