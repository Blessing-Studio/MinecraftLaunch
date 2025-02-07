using MinecraftLaunch.Base.Models.Game;
using MinecraftLaunch.Base.Models.JsonConverter;
using MinecraftLaunch.Extensions;
using System.ComponentModel;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MinecraftLaunch.Components.Parser;

/// <summary>
/// 官方游戏配置解析器
/// </summary>
/// <remarks>
/// 取自 launcher_profile.json
/// </remarks>
public sealed class LauncherProfileParser {
    private readonly Guid _clientToken;
    private readonly string _minecraftPath;

    private LauncherProfileEntry _launcherProfile;

    public Dictionary<string, GameProfileEntry> Profiles { get; } = [];

    public LauncherProfileParser(string minecraftPath, Guid clientToken = default) {
        _clientToken = clientToken;
        _minecraftPath = minecraftPath;

        try {
            Parse();
        } catch (JsonException) {
            var launcherProfile = new LauncherProfileEntry {
                Profiles = [],
                ClientToken = _clientToken.ToString("D"),
                LauncherVersion = new LauncherVersionEntry {
                    Format = 114514,
                    Name = "MinecraftLaunch"
                },
            };

            _launcherProfile = launcherProfile;
            string profileJson = _launcherProfile.Serialize(new LauncherProfileEntryContext(Get()).LauncherProfileEntry);

            if (!Directory.Exists(_minecraftPath))
                Directory.CreateDirectory(_minecraftPath);

            File.WriteAllText(Path.Combine(_minecraftPath, "launcher_profiles.json"), profileJson);
        }
    }

    public void Add(GameProfileEntry entry) {
        Profiles.TryAdd(entry.Name, entry);
    }

    public void Remove(GameProfileEntry entry) {
        Profiles.Remove(entry.Name);
    }

    public LauncherProfileEntry Parse() {
        var filePath = Path.Combine(_minecraftPath, "launcher_profiles.json");

        if (File.Exists(filePath)) {
            var launcherProfileJson = File.ReadAllText(filePath, Encoding.UTF8);
            _launcherProfile = launcherProfileJson.Deserialize(new LauncherProfileEntryContext(Get()).LauncherProfileEntry);

            Profiles.Clear();
            foreach (var profile in _launcherProfile.Profiles)
                Profiles.Add(profile.Key, profile.Value);

            return _launcherProfile;
        }

        var launcherProfile = new LauncherProfileEntry {
            Profiles = [],
            ClientToken = _clientToken.ToString("D"),
            LauncherVersion = new LauncherVersionEntry {
                Format = 114514,
                Name = "MinecraftLaunch"
            },
        };

        _launcherProfile = launcherProfile;
        string profileJson = _launcherProfile.Serialize(new LauncherProfileEntryContext(Get()).LauncherProfileEntry);

        if (!Directory.Exists(_minecraftPath))
            Directory.CreateDirectory(_minecraftPath);

        File.WriteAllText(filePath, profileJson);
        return _launcherProfile;
    }

    public Task SaveAsync(CancellationToken cancellationToken = default) {
        var filePath = Path.Combine(_minecraftPath, "launcher_profiles.json");
        _launcherProfile.Profiles = Profiles;

        var json = _launcherProfile?.Serialize(new LauncherProfileEntryContext(Get()).LauncherProfileEntry);
        return File.WriteAllTextAsync(filePath, json, cancellationToken);
    }

    #region Privates

    private JsonSerializerOptions Get() {
        var options = new JsonSerializerOptions {
            MaxDepth = 100,
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Converters = {
                new DateTimeJsonConverter()
            }
        };
        
        return options;
    }

    #endregion
}