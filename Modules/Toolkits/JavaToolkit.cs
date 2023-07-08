using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;
using MinecraftLaunch.Modules.Models.Launch;

namespace MinecraftLaunch.Modules.Toolkits;

public sealed class JavaToolkit {
    [SupportedOSPlatform("OSX")]
    private const string MacJavaHomePath = "/Library/Java/JavaVirtualMachines";

    [SupportedOSPlatform("Linux")]
    private static readonly string[] LinuxJavaHomePaths = { "/usr/lib/jvm", "/usr/lib32/jvm", ".usr/lib64/jvm" };

    public static IEnumerable<JavaInfo> GetJavas() {
        try {
            if (EnvironmentToolkit.IsWindow) {
                return GetWindowsJavas();
            } else if (EnvironmentToolkit.IsMac) {
                return GetMacJava();
            } else {
                return GetLinuxJava();
            }
        }
        catch (Exception) {
        }

        return null!;
    }

    public static JavaInfo GetJavaInfo(string javapath) {
        FileInfo info = new(javapath);

        if (javapath.IsDirectory()) {
            info = new(Path.Combine(javapath,EnvironmentToolkit.IsWindow ? "java.exe" : "java"));
        }

        try {
            int? ires = null;
            string tempinfo = null;
            string pattern = "java version \"\\s*(?<version>\\S+)\\s*\"";

            using Process Program = new Process {
                StartInfo = new() {
                    Arguments = "-version",
                    FileName = info.FullName,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                }
            };

            Program.Start();
            Program.WaitForExit(8000);
            StreamReader res = Program.StandardError;
            bool end = false;
            while (res.Peek() != -1) {
                string temp = res.ReadLine();
                if (temp.Contains("java version")) {
                    tempinfo = new Regex(pattern).Match(temp).Groups["version"].Value;
                } else if (temp.Contains("openjdk version")) {
                    pattern = pattern.Replace("java", "openjdk");
                    tempinfo = new Regex(pattern).Match(temp).Groups["version"].Value;
                } else if (temp.Contains("64-Bit")) {
                    end = true;
                }
            }

            string[] sres = tempinfo.Split(".");
            if (sres.Length != 0) {
                ires = ((int.Parse(sres[0]) == 1) ? new int?(int.Parse(sres[1])) : new int?(int.Parse(sres[0])));
            }

            return new JavaInfo {
                Is64Bit = end,
                JavaDirectoryPath = info.Directory!.FullName,
                JavaSlugVersion = Convert.ToInt32(ires),
                JavaVersion = tempinfo,
                JavaPath = info.FullName,
            };
        }
        catch (Exception) {
            return null!;
        }
    }

    public static JavaInfo GetCorrectOfGameJava(IEnumerable<JavaInfo> Javas, GameCore gameCore) {
        JavaInfo result = null;
        foreach (JavaInfo j in Javas) {
            if (j.JavaSlugVersion == gameCore.JavaVersion && j.Is64Bit) {
                result = j;
            }
        }

        if (result == null) {
            foreach (JavaInfo i in Javas) {
                if (i.JavaSlugVersion == gameCore.JavaVersion) {
                    result = i;
                }
            }
            return result;
        }

        return result;
    }

    [SupportedOSPlatform("OSX")]
    private static IEnumerable<JavaInfo> GetMacJava() {
        foreach (var i in Directory.EnumerateDirectories(MacJavaHomePath).AsParallel()) {
            if (!Directory.Exists(i + "/Contents/Home/bin"))
                continue;

            if ($"{i}/Contents/Home/bin/java".IsFile()) {
                yield return GetJavaInfo($"{i}/Contents/Home/bin/java");
            }
        }
    }

    [SupportedOSPlatform("Linux")]
    private static IEnumerable<JavaInfo> GetLinuxJava() {
        //包管理器目录下已安装的java
        foreach (var LinuxJavaHomePath in LinuxJavaHomePaths.AsParallel()) {
            if (!Directory.Exists(LinuxJavaHomePath)) {
                continue;
            }

            foreach (var jvmPath in Directory.EnumerateDirectories(LinuxJavaHomePath).AsParallel()) {
                if ($"{jvmPath}/bin/java".IsFile()) {
                    yield return GetJavaInfo($"{jvmPath}/bin/java");
                }
            }
        }

        //设置了环境变量的java
        using var cmd = new Process {
            StartInfo = new("which", "java") {
                WorkingDirectory = Directory.GetCurrentDirectory(),
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
            }
        };
        cmd.Start();
        var envJvmPath = cmd.StandardError.ReadToEnd();

        cmd.Close();
        if (envJvmPath.IsFile()) {
            yield return GetJavaInfo(envJvmPath);
        }
    }

    [SupportedOSPlatform("Windows")]
    private static IEnumerable<JavaInfo> GetWindowsJavas() {
        try {
            string? environmentVariable = Environment.GetEnvironmentVariable("Path");
            List<string> results = new List<string>();
            string[] array = environmentVariable!.Split(Path.PathSeparator);

            foreach (string obj in array) {
                string text = obj.Trim();
                if (File.Exists(Path.Combine(text, "javaw.exe"))) {
                    results.Add(text);
                }
            }

            DriveInfo[] drives = DriveInfo.GetDrives();
            for (int i = 0; i < drives.Length; i++) {
                SearchJavaInFolder(new DirectoryInfo(drives[i].Name), ref results, source: false);
            }

            SearchJavaInFolder(new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)), ref results, source: false);
            SearchJavaInFolder(new DirectoryInfo(AppDomain.CurrentDomain.SetupInformation.ApplicationBase), ref results, source: false, isFullSearch: true);
            List<string> list = new List<string>();
            foreach (string item in results) {
                FileSystemInfo fileSystemInfo = new FileInfo(Path.Combine(item, "javaw.exe"));
                do {
                    if (!fileSystemInfo.Attributes.HasFlag(FileAttributes.ReparsePoint)) {
                        fileSystemInfo = ((fileSystemInfo is FileInfo) ? ((FileInfo)fileSystemInfo).Directory : ((DirectoryInfo)fileSystemInfo).Parent)!;
                    }
                }
                while (fileSystemInfo != null);
                list.Add(item);
            }

            if (list.Count > 0) {
                results = list;
            }

            List<string> list2 = new List<string>();
            foreach (string item2 in results) {
                if (!item2.Contains("javapath_target_")) {
                    list2.Add(item2);
                }
            }

            if (list2.Count > 0) {
                results = list2;
            }

            results.Sort((string x, string s) => x.CompareTo(s));
            foreach (string item3 in results) {
                JavaInfo javaInfo = GetJavaInfo(item3);
                yield return new JavaInfo {
                    Is64Bit = javaInfo.Is64Bit,
                    JavaDirectoryPath = item3,
                    JavaSlugVersion = javaInfo.JavaSlugVersion,
                    JavaVersion = javaInfo.JavaVersion,
                    JavaPath = Path.Combine(item3, "javaw.exe")
                };
            }
        }
        finally {
            GC.Collect();
        }

        string GetDirectoryNameFromPath(string directoryPath) {
            if (directoryPath.EndsWith(Path.VolumeSeparatorChar + Path.DirectorySeparatorChar.ToString())
                || directoryPath.EndsWith(Path.VolumeSeparatorChar + Path.AltDirectorySeparatorChar.ToString())) {
                return directoryPath.Substring(0, 1);
            }

            if (directoryPath.EndsWith(Path.DirectorySeparatorChar.ToString()) || directoryPath.EndsWith(Path.AltDirectorySeparatorChar.ToString())) {
                directoryPath = directoryPath.Substring(0, directoryPath.Length - 1);
            }

            return Path.GetFileName(directoryPath);
        }

        void SearchJavaInFolder(DirectoryInfo originalPath, ref List<string> results, bool source, bool isFullSearch = false) {
            try {
                if (!originalPath.Exists) {
                    return;
                }

                string text = originalPath.FullName;
                if (!text.EndsWith(Path.DirectorySeparatorChar)) {
                    text += Path.DirectorySeparatorChar;
                }

                if (File.Exists(Path.Combine(text, "javaw.exe"))) {
                    results.Add(text);
                }

                foreach (DirectoryInfo item in originalPath.EnumerateDirectories()) {
                    if (!item.Attributes.HasFlag(FileAttributes.ReparsePoint)) {
                        string text2 = GetDirectoryNameFromPath(item.Name).ToLower();
                        var searchTerms = new List<string> { "java", "jdk", "env", "环境", "run", "软件", "jre", "bin", "mc", "software", "cache", "temp", "corretto", "roaming", "users", "craft", "program", "世界", "net", "游戏", "oracle", "game", "file", "data", "jvm", "服务", "server", "客户", "client", "整合", "应用", "运行", "前置", "mojang", "官启", "新建文件夹", "eclipse", "microsoft", "hotspot" };
                        if (isFullSearch || item.Parent!.Name.ToLower() == "users" || searchTerms.Any(text2.Contains) || text2 == "bin") {
                            SearchJavaInFolder(item, ref results, source);
                        }
                    }
                }
            }
            catch (Exception) {
            }
        }
    }
}
