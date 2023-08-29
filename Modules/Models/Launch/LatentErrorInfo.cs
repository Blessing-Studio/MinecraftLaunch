using MinecraftLaunch.Modules.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftLaunch.Modules.Models.Launch {
    public class LatentErrorInfo {
        public string Message { get; init; }

        public LatentErrorType Type { get; set; }

        public static LatentErrorInfo Build(string message, LatentErrorType type) {
            return new() {
                Message = message,
                Type = type
            };
        }
    }
}
