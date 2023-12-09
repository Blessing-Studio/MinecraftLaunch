using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftLaunch.Extensions {
    public static class CryptoExtension {
        public static IEnumerable<byte> Remove(this ReadOnlySpan<byte> data) {
            if (data.Length == 0 || data[0] != 239 || data[1] != 187 || data[2] != 191) {
                return data.ToArray();
            }

            return data.Slice(3).ToArray();
        }

        public static ReadOnlySpan<byte> Remove(this ReadOnlySpan<byte> data, int i = 2) {
            if (data.Length > 2 && data[0] == 239 && data[1] == 187 && data[2] == 191) {
                return data.Slice(3);
            }

            return data;
        }
    }
}
