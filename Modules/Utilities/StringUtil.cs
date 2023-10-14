using NPinyin;
using System.Text;
using System.IO.Compression;
using System.Text.RegularExpressions;
using Microsoft.International.Converters.PinYinConverter;

namespace MinecraftLaunch.Modules.Utilities {
    public class StringUtil {
        public static string GetSpell(string text) {
            if (string.IsNullOrEmpty(text)) {
                return string.Empty;
            }

            StringBuilder fullSpell = new StringBuilder();
            foreach (char chr in text) {
                fullSpell.Append(GetSpell(chr));
            }

            return fullSpell.ToString().ToUpper();
        }

        public static string GetFirstSpell(string chineseText) {
            if (string.IsNullOrEmpty(chineseText)) {
                return string.Empty;
            }

            StringBuilder firstSpell = new StringBuilder();
            foreach (char chr in chineseText) {
                firstSpell.Append(GetSpell(chr)[0]);
            }

            return firstSpell.ToString().ToUpper();
        }

        public static string ConvertGzipStreamToString(Stream stream) {
            using GZipStream gzipStream = new(stream, CompressionMode.Decompress);
            using StreamReader reader = new StreamReader(gzipStream);
            return reader.ReadToEnd();
        }

        public static IEnumerable<string> ConvertGzipStreamToList(Stream stream) {
            using GZipStream gzipStream = new(stream, CompressionMode.Decompress);
            using StreamReader reader = new StreamReader(gzipStream);

            string line;
            while ((line = reader.ReadLine()!) != null) {
                yield return line;
            }
        }

        public static string GetPropertyFromHtmlText(string htmlText, string tagName, string attributeName) {
            string pattern = $"<{tagName}[^>]*?{attributeName}=(['\"\"]?)(?<url>[^'\"\"\\s>]+)\\1[^>]*>";
            Match match = Regex.Match(htmlText, pattern, RegexOptions.IgnoreCase);

            return match.Groups["url"].Value;
        }

        private static string GetSpell(char chr) {
            var convertedChar = Pinyin.GetPinyin(chr);

            if (ChineseChar.IsValidChar(convertedChar[0])) {
                ChineseChar chineseChar = new(convertedChar[0]);

                foreach (string pinyin in chineseChar.Pinyins) {
                    if (!string.IsNullOrEmpty(pinyin)) {
                        return pinyin.Remove(pinyin.Length - 1, 1);
                    }
                }
            }

            return convertedChar;
        }
    }
}
