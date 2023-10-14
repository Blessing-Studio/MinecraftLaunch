using System;
using System.Collections.Generic;
using System.Text;

namespace MinecraftLaunch.Modules.Utilities;

public class CryptoUtil {
    public static IEnumerable<byte> Remove(ReadOnlySpan<byte> data) {
        if (data.Length == 0 || data[0] != 239 || data[1] != 187 || data[2] != 191) {
            return data.ToArray();
        }

        return data.Slice(3).ToArray();
    }

    public static ReadOnlySpan<byte> Remove(ReadOnlySpan<byte> data, int i = 2) {
        if (data.Length > 2 && data[0] == 239 && data[1] == 187 && data[2] == 191) {
            return data.Slice(3);
        }

        return data;
    }

    public static string DecryptOfCaesar(string encryptedData, int key = 1) {
        return new string(encryptedData.Select(c => (char)(c - key)).ToArray());
    }

    public static string EncryptOfCaesar(string original, int key = 1) {
        return new string(original.Select(c => (char)(c + key)).ToArray());
    }
}
