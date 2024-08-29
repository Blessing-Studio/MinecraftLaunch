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

    public static string Replace(this string text, IDictionary<string, string> keyValuePairs) {
        string replacedText = text;
        foreach (var item in keyValuePairs) {
            replacedText = replacedText.Replace(item.Key, item.Value);
        }

        return replacedText;
    }

    public static DownloadRequest ToDownloadRequest(this string url, FileInfo path) {
        return new DownloadRequest {
            Url = url,
            FileInfo = path,
        };
    }

    public static IEnumerable<string> GroupArguments(this IEnumerable<string> parameters) {
        var queue = new Queue<string>(parameters);
        var group = new List<string>();

        while (queue.Count > 0) {
            var next = queue.Dequeue();

            if (group.Count == 0) {
                group.Add(next);
            } else {
                if (group.First().StartsWith('-') && next.StartsWith('-')) {
                    yield return string.Join(group.First().EndsWith('=') ? "" : " ", group);
                    group.Clear();
                }
                group.Add(next);
            }
        }

        if (group.Count > 0) {
            yield return string.Join(group.First().EndsWith('=') ? "" : " ", group);
        }
    }
}