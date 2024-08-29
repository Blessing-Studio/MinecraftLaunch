namespace MinecraftLaunch.Extensions;

public static class CryptoExtension {

    public static string BytesToString(this byte[] bytes) {
        return BitConverter.ToString(bytes).Replace("-", string.Empty);
    }

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