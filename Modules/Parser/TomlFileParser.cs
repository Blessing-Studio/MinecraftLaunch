using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MinecraftLaunch.Modules.Parser {
    public sealed class TomlFileParser {
        private List<string> CurrentGroup = new();

        public string Content { get; set; }

        public Dictionary<string, List<string>> Groups { get; set; } = new();

        public TomlFileParser(string content) {
            Content = content;
            Parse();
        }

        public string this[string key] {       
            get {           
                return GetString(key);
            }
        }

        public string GetString(string key) {
            Regex regex = new Regex("(?<=(" + "\"" + "))[.\\s\\S]*?(?=(" + "\"" + "))");
            return regex.Match(CurrentGroup.Where(x => x.Contains(key))
                .FirstOrDefault()!).Value;
        }

        public bool Select(string groupId) {
            if (Groups.ContainsKey(groupId)) {
                CurrentGroup = Groups[groupId];
                return true;
            }

            return false;
        }

        public void Parse() {
            var allLine = new List<string>();
            var reader = new StringReader(Content);
            string content;

            List<string> linecache = new();
            bool flag = false;
            while ((content = reader.ReadLine()!) != null) {
                if (!string.IsNullOrEmpty(content) && content.First() != '#') {
                    if (content.Contains("'''") || flag) {
                        linecache.Add(content);

                        //遇到第一个时
                        if (!flag) {
                            flag = true;
                        } else if (content.Contains("'''") && flag) {//遇到第二个时
                            flag = false;
                            allLine.Add(string.Join("", linecache).Replace("'''", "\""));
                        }
                        continue;
                    }

                    allLine.Add(content);
                }
            }

            string groupId = "none";
            var cache = new List<string>();
            var regex = new Regex("(?<=\\[\\[).+?(?=\\]\\])");

            foreach (var line in allLine) {
                if (regex.IsMatch(line)) {
                    if (Groups.ContainsKey(groupId)) {

                    }

                    Groups.Add(groupId, cache.ToList());
                    groupId = regex.Match(line).Value;

                    cache.Clear();
                } else {
                    cache.Add(line);
                }
            }

            //处理最后一个节点下的值
            if(cache.Count > 0) {
                if (Groups.ContainsKey(groupId)) {
                    Groups[groupId].AddRange(cache);
                    cache.Clear();
                }
            }
        }

        public IEnumerable<string> GetStrings(string key) {
            Regex regex = new Regex("(?<=(" + "\"" + "))[.\\s\\S]*?(?=(" + "\"" + "))");
            return CurrentGroup.Where(x => x.Contains(key)).Select(x => regex.Match(x).Value);
        }
    }
}

//foreach (var line in allLine) {
//    if (regex.IsMatch(line)) {
//        var key = regex.Match(line).Value;

//        if (Groups.Count == 0) {
//            firstGroupId = key;
//            Groups.Add(firstGroupId, cache);
//        } else if (!Groups.ContainsKey(key)) {
//            Groups.Add(key, cache);
//        }
//    } else {
//        cache.Add(line);
//    }
//}