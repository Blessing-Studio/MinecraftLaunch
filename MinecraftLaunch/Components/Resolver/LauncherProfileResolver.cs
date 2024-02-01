using System.Text;
using MinecraftLaunch.Utilities;
using MinecraftLaunch.Extensions;
using MinecraftLaunch.Classes.Interfaces;
using MinecraftLaunch.Classes.Models.Game;

namespace MinecraftLaunch.Components.Resolver;

/// <summary>
/// 官方游戏配置解析器
/// </summary>
/// <remarks>
/// 取自 launcher_profile.json
/// </remarks>
public sealed class LauncherProfileResolver(string rootPath, Guid clientToken = default) : IResolver<LauncherProfileEntry> {
    private readonly Guid _clientToken = clientToken;
    private readonly string _proFilePath = rootPath.OfLauncherProfilePath();

    public LauncherProfileEntry LauncherProfile { get; set; }
    
    public void SaveProfile() {
        var launcherProfileJson = LauncherProfile.Serialize(typeof(LauncherProfileEntry), 
            new LauncherProfileEntryContext(JsonConverterUtil.DefaultJsonOptions));
        
        File.WriteAllText(_proFilePath, launcherProfileJson);
    }

    public bool HasProfile(string name) {
        return LauncherProfile.Profiles.Any(x => x.Value.Name.Equals(name, StringComparison.Ordinal));
    }
    
    public bool RemoveProfile(string name) {
        return LauncherProfile.Profiles.Remove(name);
    }

    public GameProfileEntry GetProfile(string name) {
        var profile = LauncherProfile.Profiles.FirstOrDefault(pe => 
            pe.Value.Name.Equals(name, StringComparison.Ordinal)).Value;

        if (profile == null) {
            throw new InvalidOperationException($"未找到名为 '{name}' 的游戏配置");
        }

        profile.Resolution ??= new ResolutionEntry();
        return profile;
    }
    
    public bool AddProfile(GameProfileEntry gameProfile) { 
        return LauncherProfile.Profiles.TryAdd(gameProfile.Name, gameProfile);
    }

    public LauncherProfileEntry Resolve(string str = default) {
        if (File.Exists(_proFilePath)) {
            var launcherProfileJson = File.ReadAllText(_proFilePath, Encoding.UTF8);
            LauncherProfile = launcherProfileJson.Deserialize(LauncherProfileEntryContext
                .Default.LauncherProfileEntry);
            return LauncherProfile;
        }
        
        var launcherProfile = new LauncherProfileEntry {
            Profiles = new(),
            ClientToken = _clientToken.ToString("D"),
            LauncherVersion = new LauncherVersionEntry {
                Format = 114514,
                Name = "下北泽"
            },
        };
        
        LauncherProfile = launcherProfile;
        string profileJson = LauncherProfile.Serialize(typeof(LauncherProfileEntry), 
            new LauncherProfileEntryContext(JsonConverterUtil.DefaultJsonOptions));

        if (!Directory.Exists(rootPath)) {
            Directory.CreateDirectory(rootPath);
        }

        File.WriteAllText(_proFilePath, profileJson);          
        return LauncherProfile;
    }
}