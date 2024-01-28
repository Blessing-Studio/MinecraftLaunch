using System.Text.Json.Serialization;

namespace MinecraftLaunch.Classes.Models.Auth;

public sealed record LauncherProfileEntry {
    /// <summary>
    /// 客户端令牌
    /// </summary>
    [JsonPropertyName("clientToken")] 
    public string ClientToken { get; set; }

    /// <summary>
    /// 活动账户信息
    /// </summary>
    [JsonPropertyName("selectedUser")] 
    public SelectedUserEntry SelectedAccount { get; set; }

    /// <summary>
    /// 启动器版本信息
    /// </summary>
    [JsonPropertyName("launcherVersion")]
    public LauncherVersionEntry LauncherVersion { get; set; }

    /// <summary>
    /// 档案信息
    /// </summary>
    [JsonPropertyName("profiles")] 
    public Dictionary<string, GameProfileEntry> Profiles { get; set; }
}

public sealed record GameProfileEntry {
    /// <summary>
    ///     名称
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    ///     游戏目录
    /// </summary>
    [JsonPropertyName("gameDir")]
    public string GameDir { get; set; }

    /// <summary>
    ///     创建时间
    /// </summary>
    [JsonPropertyName("created")]
    public DateTime Created { get; set; }

    /// <summary>
    ///     Java虚拟机路径
    /// </summary>
    [JsonPropertyName("javaDir")]
    public string JavaDir { get; set; }

    /// <summary>
    ///     游戏窗口分辨率
    /// </summary>
    [JsonPropertyName("resolution")]
    public ResolutionEntry? Resolution { get; set; }

    /// <summary>
    ///     游戏图标
    /// </summary>
    [JsonPropertyName("icon")]
    public string Icon { get; set; }

    /// <summary>
    ///     Java虚拟机启动参数
    /// </summary>
    [JsonPropertyName("javaArgs")]
    public string JavaArgs { get; set; }

    /// <summary>
    ///     最后一次的版本Id
    /// </summary>
    [JsonPropertyName("lastVersionId")]
    public string LastVersionId { get; set; }

    /// <summary>
    ///     最后一次启动
    /// </summary>
    [JsonPropertyName("lastUsed")]
    public DateTime LastUsed { get; set; }

    /// <summary>
    ///     版本类型
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; }
}

public sealed record SelectedUserEntry {
    [JsonPropertyName("account")] 
    public string Account { get; set; }

    [JsonPropertyName("profile")]
    public string Profile { get; set; }
}

public sealed record LauncherVersionEntry {
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("format")]
    public int Format { get; set; }
}

public sealed record ResolutionEntry
{
    [JsonPropertyName("width")] 
    public uint Width { get; set; }

    [JsonPropertyName("height")] 
    public uint Height { get; set; }

    [JsonIgnore]
    public bool FullScreen { get; set; }

    public bool IsDefault() {
        return Width == 0 && Height == 0;
    }
}

[JsonSerializable(typeof(LauncherProfileEntry))]
sealed partial class LauncherProfileEntryContext : JsonSerializerContext;