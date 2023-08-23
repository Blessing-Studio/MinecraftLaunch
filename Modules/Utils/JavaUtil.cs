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

namespace MinecraftLaunch.Modules.Utils;

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
        FileInfo info = new(javaPath);

        if (javaPath.IsDirectory()) {
            info = new(Path.Combine(javaPath, EnvironmentUtil.IsWindow ? "java.exe" : "java"));

            if (!info.Exists) {
                info = new(Path.Combine(javaPath, EnvironmentUtil.IsWindow ? "javaw.exe" : "java"));
            }
        }

        if (EnvironmentUtil.IsWindow) {
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(info.FullName);

            return new() {
                Is64Bit = GetIs64Bit(),
                JavaDirectoryPath = info.Directory!.FullName,
                JavaSlugVersion = fileVersionInfo.ProductMajorPart,
                JavaVersion = fileVersionInfo.ProductVersion!,
                JavaPath = info.FullName,
            };
        } else {
            try {
                int? ires = null;
                string tempinfo = null;
                string pattern = "java version \"\\s*(?<version>\\S+)\\s*\"";

                using var Program = new Process {
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
                    string temp = res.ReadLine()!;
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

        bool GetIs64Bit() {
            ushort architecture = 0;

            try {
                using var fStream = new FileStream(info.FullName, FileMode.Open, FileAccess.Read);
                using var bReader = new BinaryReader(fStream);

                if (bReader.ReadUInt16() == 23117) {
                    fStream.Seek(0x3A, SeekOrigin.Current);
                    fStream.Seek(bReader.ReadUInt32(), SeekOrigin.Begin);

                    if (bReader.ReadUInt32() == 17744) {
                        fStream.Seek(20, SeekOrigin.Current);
                        architecture = bReader.ReadUInt16();
                    }
                }
            }
            catch { }

            return architecture is 523 || architecture is 267;
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
        //��������Ŀ¼���Ѱ�װ��java
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

        //�����˻���������java
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
                throw new Exception("�������ļ�����" + filePath);
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
                        var searchTerms = new List<string> { "java", "jdk", "jbr", "bin", "env", "����", "run", "���", "jre", "bin", "mc", "software", "cache", "temp", "corretto", "roaming", "users", "craft", "program", "����", "net", "��Ϸ",
                    "oracle", "game", "file", "data", "jvm", "����", "server", "�ͻ�", "client", "����", "Ӧ��", "����", "ǰ��", "mojang", "����", "�½��ļ���", "eclipse", "microsoft", "hotspot" ,"idea", "android",  };
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
