using MinecraftLaunch.Classes.Models.Download;

namespace MinecraftLaunch.Extensions;

public static class StringExtension {
    public static bool IsUrl(this string str) {
        Uri uriResult;
        bool result = Uri.TryCreate(str, UriKind.Absolute, out uriResult)
                      && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        return result;
    }

    public static string ToPath(this string raw) {
        if (!Enumerable.Contains(raw, ' ')) {
            return raw;
        }
        return "\"" + raw + "\"";
    }

    public static string Replace(this string text, Dictionary<string, string> keyValuePairs) {
        string replacedText = text;
        foreach (var item in keyValuePairs) {
            replacedText = replacedText.Replace(item.Key, item.Value);
        }

        return replacedText;
    }

    public static DownloadRequest ToDownloadRequest(this string url, string path) {
        return new DownloadRequest {
            Url = url,
            Path = path,
            IsCompleted = false,
            DownloadedBytes = 0,
            Name = Path.GetFileName(path),
        };
    }
}