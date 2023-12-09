using MinecraftLaunch.Classes.Enums;

namespace MinecraftLaunch.Classes.Models.Auth {
    public abstract record Account {
        public virtual AccountType Type { get; init; }

        public string Name { get; set; }

        public Guid Uuid { get; set; }

        public string AccessToken { get; set; }
    }

    public record MicrosoftAccount : Account {
        public override AccountType Type => AccountType.Microsoft;

        public string RefreshToken { get; set; }

        public DateTime LastRefreshTime { get; set; }
    }

    public record YggdrasilAccount : Account {
        public override AccountType Type => AccountType.Yggdrasil;

        public string YggdrasilServerUrl { get; set; }

        public string ClientToken { get; set; }
    }

    public record OfflineAccount : Account {
        public override AccountType Type => AccountType.Offline;
    }
}
