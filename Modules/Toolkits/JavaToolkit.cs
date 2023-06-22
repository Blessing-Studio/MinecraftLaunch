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
    
    public static JavaInfo GetJavaInfo(string javapath) {
        FileInfo info = new(javapath);
        try {
            int? ires = null;
            string tempinfo = null;
            string pattern = "java version \"\\s*(?<version>\\S+)\\s*\"";

            using Process Program = new Process {
                StartInfo = new() {
                    Arguments = "-version",
                    FileName = javapath.EndsWith(".exe") ? Path.Combine(info.Directory!.FullName, "java.exe") : info.FullName,
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
                if (temp.Contains("java version"))
                    tempinfo = new Regex(pattern).Match(temp).Groups["version"].Value;
                else if (temp.Contains("openjdk version")) {
                    pattern = pattern.Replace("java", "openjdk");
                    tempinfo = new Regex(pattern).Match(temp).Groups["version"].Value;
                } else if (temp.Contains("64-Bit"))
                    end = true;
            }

            string[] sres = tempinfo.Split(".");
            if (sres.Length != 0)
                ires = ((int.Parse(sres[0]) == 1) ? new int?(int.Parse(sres[1])) : new int?(int.Parse(sres[0])));

            return new JavaInfo {
                Is64Bit = end,
                JavaDirectoryPath = info.Directory!.FullName,
                JavaSlugVersion = Convert.ToInt32(ires),
                JavaVersion = tempinfo,
                JavaPath = info.FullName,
            };
        }
        catch (Exception) {
            return null;
        }
    }

    public static IEnumerable<JavaInfo> GetJavas() {
        List<string> temp = new();
        List<JavaInfo> ret = new();

        try {
            if (EnvironmentToolkit.IsWindow) {
                foreach (var item in DriveInfo.GetDrives().AsParallel()) {
                    temp.AddRange(addSubDirectory(new DirectoryInfo(item.Name), "javaw.exe").Where(File.Exists));
                }
                GC.Collect();

                foreach (var i in temp.AsParallel()) {
                    ret.Add(GetJavaInfo(i));
                }

                return ret;
            } else if (EnvironmentToolkit.IsMac) {
                return GetMacJava();
            } else {
                return GetLinuxJava();
            }
        }
        catch (Exception ex) { }

        return null!;
    }

    public static JavaInfo GetCorrectOfGameJava(IEnumerable<JavaInfo> Javas, GameCore gameCore) {
        JavaInfo res = null;
        foreach (JavaInfo j in Javas) {
            if (j.JavaSlugVersion == gameCore.JavaVersion && j.Is64Bit) {
                res = j;
            }
        }
        if (res == null) {
            foreach (JavaInfo i in Javas) {
                if (i.JavaSlugVersion == gameCore.JavaVersion) {
                    res = i;
                }
            }
            return res;
        }
        return res;
    }

    static List<string> addSubDirectory(DirectoryInfo directory, string pattern) {
        List<string> files = new List<string>();
        try {
            foreach (FileInfo fi in directory.GetFiles(pattern).AsParallel()) {
                files.Add(fi.FullName);
            }

            foreach (DirectoryInfo di in directory.GetDirectories().AsParallel()) {
                addSubDirectory(di, pattern);
            }
        }
        catch {
        }
        finally {
            GC.Collect();
        }

        return files;
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

    [SupportedOSPlatform("LINUX")]
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
}
