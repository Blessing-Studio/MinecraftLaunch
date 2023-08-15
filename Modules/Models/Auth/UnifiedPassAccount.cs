using MinecraftLaunch.Modules.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftLaunch.Modules.Models.Auth {
    public class UnifiedPassAccount : Account {
        public string ServerId { get; set; } = string.Empty;

        public override AccountType Type => AccountType.UnifiedPass;
    }
}
