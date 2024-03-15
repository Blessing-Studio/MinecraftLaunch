using System.Security.Cryptography;
using MinecraftLaunch.Classes.Interfaces;
using MinecraftLaunch.Classes.Models.Download;

namespace MinecraftLaunch.Extensions;

public static class DownloadEntryExtension {
    private static readonly char[] _hexTable =
        { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };

    public static DownloadRequest ToDownloadRequest(this IDownloadEntry entry) {
        return new DownloadRequest {
            Url = entry.Url,
            FileInfo = entry.Path.ToFileInfo()
        };
    }

    public static bool Verify(this IDownloadEntry entry) {
        if(entry == null) 
            return true;

        if(!File.Exists(entry.Path))
            return false;

        using var sha1Provider = SHA1.Create();
        using var fileStream = File.OpenRead(entry.Path);
        byte[] sha1Bytes = sha1Provider.ComputeHash(fileStream);
        ReadOnlySpan<char> sha1 = entry.Checksum;

        for (int i = 0; i < sha1Bytes.Length; i++) {
            char c0 = _hexTable[sha1Bytes[i] >> 4];
            char c1 = _hexTable[sha1Bytes[i] & 0x0F];

            if (c0 != sha1[2 * i] || c1 != sha1[2 * i + 1]) {
                return false;
            }
        }

        return true;
    }
}