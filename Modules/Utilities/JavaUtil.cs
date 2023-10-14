using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;
using MinecraftLaunch.Modules.Models.Launch;

namespace MinecraftLaunch.Modules.Utilities;

public sealed class JavaUtil {
    [SupportedOSPlatform("OSX")]
    private const string MacJavaHomePath = "/Library/Java/JavaVirtualMachines";

    [SupportedOSPlatform("Linux")]
    private static readonly string[] LinuxJavaHomePaths = { "/usr/lib/jvm", "/usr/lib32/jvm", ".usr/lib64/jvm" };

    public static IEnumerable<JavaInfo> GetJavas() {
        try {
            if (EnvironmentUtil.IsWindow) {
                return GetWindowsJavas();
            } else if (EnvironmentUtil.IsMac) {
                return GetMacJava();
            } else {
                return GetLinuxJava();
            }
        }
        catch (Exception) {
        }

        return null!;
    }

    public static JavaInfo GetJavaInfo(string javaPath) {
        FileInfo javaFileInfo = new(javaPath);

        if (javaPath.IsDirectory()) {
            javaFileInfo = new FileInfo(Path.Combine(javaPath, EnvironmentUtil.IsWindow ? "java.exe" : "java"));

            if (!javaFileInfo.Exists) {
                javaFileInfo = new FileInfo(Path.Combine(javaPath, EnvironmentUtil.IsWindow ? "javaw.exe" : "java"));
            }
        }

        if (EnvironmentUtil.IsWindow) {
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(javaFileInfo.FullName);

            return new JavaInfo {
                Is64Bit = GetIs64Bit(),
                JavaDirectoryPath = javaFileInfo.Directory!.FullName,
                JavaSlugVersion = fileVersionInfo.ProductMajorPart,
                JavaVersion = fileVersionInfo.ProductVersion!,
                JavaPath = javaFileInfo.FullName,
            };
        } else {
            try {
                int? versionNumber = null;
                string versionInfo = null;
                string pattern = "java version \"\\s*(?<version>\\S+)\\s*\"";

                using var program = new Process {
                    StartInfo = new ProcessStartInfo {
                        Arguments = "-version",
                        FileName = javaFileInfo.FullName,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true
                    }
                };

                program.Start();
                program.WaitForExit(8000);
                using StreamReader res = program.StandardError;
                bool is64Bit = false;

                while (res.Peek() != -1) {
                    string temp = res.ReadLine()!;
                    if (temp.Contains("java version")) {
                        versionInfo = new Regex(pattern).Match(temp).Groups["version"].Value;
                    } else if (temp.Contains("openjdk version")) {
                        pattern = pattern.Replace("java", "openjdk");
                        versionInfo = new Regex(pattern).Match(temp).Groups["version"].Value;
                    } else if (temp.Contains("64-Bit")) {
                        is64Bit = true;
                    }
                }

                string[] versionParts = versionInfo.Split(".");
                if (versionParts.Length != 0) {
                    versionNumber = (int.Parse(versionParts[0]) == 1) ? int.Parse(versionParts[1]) : int.Parse(versionParts[0]);
                }

                return new JavaInfo {
                    Is64Bit = is64Bit,
                    JavaDirectoryPath = javaFileInfo.Directory!.FullName,
                    JavaSlugVersion = Convert.ToInt32(versionNumber),
                    JavaVersion = versionInfo,
                    JavaPath = javaFileInfo.FullName,
                };
            }
            catch (Exception) {
                return null!;
            }
        }

        bool GetIs64Bit() {
            ushort architecture = 0;

            try {
                using var fileStream = new FileStream(javaFileInfo.FullName, FileMode.Open, FileAccess.Read);
                using var binaryReader = new BinaryReader(fileStream);

                if (binaryReader.ReadUInt16() == 23117) {
                    fileStream.Seek(0x3A, SeekOrigin.Current);
                    fileStream.Seek(binaryReader.ReadUInt32(), SeekOrigin.Begin);

                    if (binaryReader.ReadUInt32() == 17744) {
                        fileStream.Seek(20, SeekOrigin.Current);
                        architecture = binaryReader.ReadUInt16();
                    }
                }
            }
            catch { }

            return architecture is 523 or 267;
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
        string? environmentVariable = Environment.GetEnvironmentVariable("Path");
        List<string> results = new List<string>();
        string[] array = environmentVariable!.Split(Path.PathSeparator);

        foreach (string obj in array) {
            string text = obj.Trim(" \"".ToCharArray());
            if (!obj.EndsWith("\\")) {
                text += "\\";
            }

            if (File.Exists($"{obj}javaw.exe")) {
                results.Add(text);
            }
        }

        DriveInfo[] drives = DriveInfo.GetDrives();
        for (int i = 0; i < drives.Length; i++) {
            SearchJavaInFolder(new DirectoryInfo(drives[i].Name), ref results);
        }

        SearchJavaInFolder(new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)), ref results);
        SearchJavaInFolder(new DirectoryInfo(AppDomain.CurrentDomain.SetupInformation.ApplicationBase), ref results, isFullSearch: true);
        List<string> list = new List<string>();
        foreach (string item in results) {
            FileSystemInfo fileSystemInfo = new FileInfo(item.Replace("\\\\", "\\").Replace("/", "\\") + "javaw.exe");
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

        string GetDirectoryNameFromPath(string directoryPath) {
            if (directoryPath.EndsWith(":\\") || directoryPath.EndsWith(":\\\\")) {
                return directoryPath.Substring(0, 1);
            }

            if (directoryPath.EndsWith("\\") || directoryPath.EndsWith("/")) {
                directoryPath = Strings.Left(directoryPath, directoryPath.Length - 1);
            }

            return GetFileNameFromPath(directoryPath);
        }

        string GetFileNameFromPath(string filePath) {
            if (filePath.EndsWith("\\") || filePath.EndsWith("/")) {
                throw new Exception("不包含文件名：" + filePath);
            }

            if (!filePath.Contains("\\") && !filePath.Contains("/")) {
                return filePath;
            }

            if (filePath.Contains("?")) {
                filePath = Strings.Left(filePath, filePath.LastIndexOf("?"));
            }

            return Strings.Mid(filePath, 0)!;
        }

        void SearchJavaInFolder(DirectoryInfo originalPath, ref List<string> results, bool isFullSearch = false) {
            try {
                if (!originalPath.Exists) {
                    return;
                }

                string text = originalPath.FullName.Replace("\\\\", "\\");
                if (!text.EndsWith("\\")) {
                    text += "\\";
                }

                if (File.Exists(text + "javaw.exe")) {
                    results.Add(text);
                }

                foreach (DirectoryInfo item in originalPath.EnumerateDirectories()) {
                    if (!item.Attributes.HasFlag(FileAttributes.ReparsePoint)) {
                        string text2 = GetDirectoryNameFromPath(item.Name).ToLower();
                        var searchTerms = new List<string> { "java", "jdk", "jbr", "bin", "env", "环境", "run", "软件", "jre", "bin", "mc", "software", "cache", "temp", "corretto", "roaming", "users", "craft", "program", "世界", "net", "游戏",
                    "oracle", "game", "file", "data", "jvm", "服务", "server", "客户", "client", "整合", "应用", "运行", "前置", "mojang", "官启", "新建文件夹", "eclipse", "microsoft", "hotspot" ,"idea", "android",  };
                        if (isFullSearch || item.Parent!.Name.ToLower() == "users" || searchTerms.Any(text2.ToLower().Contains)) {
                            SearchJavaInFolder(item, ref results);
                        }
                    }
                }
            }
            catch (UnauthorizedAccessException) {
            }
        }
    }
}
