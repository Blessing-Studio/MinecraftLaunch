using Microsoft.International.Converters.PinYinConverter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftLaunch.Modules.Utils
{
    public class StringUtil
    {
        /// <summary>
        /// 获取中文拼音
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 汉字转首字母
        /// </summary>
        /// <param name="strChinese"></param>
        /// <returns></returns>
        public static string GetFirstSpell(string strChinese)
        {
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
