using Microsoft.International.Converters.PinYinConverter;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MinecraftLaunch.Modules.Utils {
    public class StringUtil {
        public static string GetSpell(string text) {
            try {
                if (text.Length != 0) {
                    StringBuilder fullSpell = new StringBuilder();
                    for (int i = 0; i < text.Length; i++) {
                        var chr = text[i];
                        fullSpell.Append(GetSpell(chr));
                    }

                    return fullSpell.ToString().ToUpper();
                }
            }
            catch (Exception e) {
                throw;
            }

            return string.Empty;
        }

        public static string GetFirstSpell(string strChinese) {
            try {
                if (strChinese.Length != 0) {
                    StringBuilder fullSpell = new StringBuilder();
                    for (int i = 0; i < strChinese.Length; i++) {
                        var chr = strChinese[i];
                        fullSpell.Append(GetSpell(chr)[0]);
                    }

                    return fullSpell.ToString().ToUpper();
                }
            }
            catch (Exception e) {
                throw;
            }

            return string.Empty;
        }

        public static string ConvertGzipStreamToString(Stream stream) {
            GZipStream gzipStream = new(stream, CompressionMode.Decompress);
            using (StreamReader reader = new StreamReader(gzipStream)) {
                return reader.ReadToEnd();
            }
        }

        public static IEnumerable<string> ConvertGzipStreamToList(Stream stream) {
            GZipStream gzipStream = new(stream, CompressionMode.Decompress);
            using (StreamReader reader = new StreamReader(gzipStream)) {
                while (reader.Peek() != -1) {
                    yield return reader.ReadLine()!;
                }
            }
        }

        public static string GetPropertyFromHtmlText(string str, string title, string attrib) {
            string tmpStr = string.Format("<{0}[^>]*?{1}=(['\"\"]?)(?<url>[^'\"\"\\s>]+)\\1[^>]*>", title, attrib);
            Match TitleMatch = Regex.Match(str, tmpStr, RegexOptions.IgnoreCase);

            string result = TitleMatch.Groups["url"].Value;
            return result;
        }

        private static string GetSpell(char chr) {
            var coverchr = NPinyin.Pinyin.GetPinyin(chr);

            bool isChineses = ChineseChar.IsValidChar(coverchr[0]);
            if (isChineses) {
                ChineseChar chineseChar = new ChineseChar(coverchr[0]);
                foreach (string value in chineseChar.Pinyins) {
                    if (!string.IsNullOrEmpty(value)) {
                        return value.Remove(value.Length - 1, 1);
                    }
                }
            }

            return coverchr;
        }
    }
}
