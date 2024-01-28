using MinecraftLaunch.Classes.Enums;
using System.Text.Json.Serialization;

namespace MinecraftLaunch.Classes.Models.Auth;

public abstract record Account {
    public virtual AccountType Type { get; init; }

    public string Name { get; set; }

    public Guid Uuid { get; set; }

    public string AccessToken { get; set; }
}

public sealed record MicrosoftAccount : Account {
    public override AccountType Type => AccountType.Microsoft;

    public string RefreshToken { get; set; }

    public DateTime LastRefreshTime { get; set; }
}

public sealed record YggdrasilAccount : Account {
    public override AccountType Type => AccountType.Yggdrasil;

    public string YggdrasilServerUrl { get; set; }

    public string ClientToken { get; set; }
}

public sealed record OfflineAccount : Account {
    public override AccountType Type => AccountType.Offline;
}

public sealed record UnifiedPassAccount : Account {
    public override AccountType Type => AccountType.UnifiedPass;

    public string ServerId { get; set; }

    public string ClientToken { get; set; }
}

[JsonSerializable(typeof(Account))]
sealed partial class AccountContext : JsonSerializerContext;