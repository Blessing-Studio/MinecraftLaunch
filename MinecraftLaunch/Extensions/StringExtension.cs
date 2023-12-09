namespace MinecraftLaunch.Extensions {
    public static class StringExtension {
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
    }
}
