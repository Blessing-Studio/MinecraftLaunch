﻿using MinecraftLaunch.Classes.Interfaces;
using MinecraftLaunch.Classes.Models.Game;
using MinecraftLaunch.Extensions;
using MinecraftLaunch.Utilities;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace MinecraftLaunch.Components.Fetcher;

/// <summary>
/// Fetches Java installations on the system.
/// </summary>
public sealed class JavaFetcher : IFetcher<ImmutableArray<JavaEntry>> {

    #region Fields

    [SupportedOSPlatform(nameof(OSPlatform.OSX))]
    private const string _macJavaHomePath = "/Library/Java/JavaVirtualMachines";

    [SupportedOSPlatform(nameof(OSPlatform.Linux))]
    private static readonly string[] _linuxJavaHomePaths = ["/usr/lib/jvm", "/usr/lib32/jvm", ".usr/lib64/jvm"];

    [SupportedOSPlatform(nameof(OSPlatform.Windows))]
    private static readonly string[] _windowJavaSearchTerms = ["java",
        "jdk",
        "jbr",
        "bin",
        "env",
        "环境",
        "run",
        "软件",
        "jre",
        "bin",
        "mc",
        "software",
        "cache",
        "temp",
        "corretto",
        "roaming",
        "users",
        "craft",
        "program",
        "世界",
        "net",
        "游戏",
        "oracle",
        "game",
        "file",
        "data",
        "jvm",
        "服务",
        "server",
        "客户",
        "client",
        "整合",
        "应用",
        "运行",
        "前置",
        "mojang",
        "官启",
        "新建文件夹",
        "eclipse",
        "microsoft",
        "hotspot",
        "idea",
        "android",
    ];

    #endregion Fields

    /// <summary>
    /// Fetches the Java installations synchronously.
    /// </summary>
    /// <returns>An immutable array of Java entries.</returns>
    public ImmutableArray<JavaEntry> Fetch() {
        return FetchAsync().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Fetches the Java installations asynchronously.
    /// </summary>
    /// <returns>A ValueTask that represents the asynchronous operation. The task result contains an immutable array of Java entries.</returns>
    public async ValueTask<ImmutableArray<JavaEntry>> FetchAsync() {
        return EnvironmentUtil.GetPlatformName() switch {
            "windows" => FetchWindowJava(),
            "osx" => await FetchMacJavaAsync(),
            "linux" => await FetchLinuxJavaAsync(),
            _ => FetchWindowJava()
        };
    }

    [SupportedOSPlatform(nameof(OSPlatform.OSX))]
    private async ValueTask<ImmutableArray<JavaEntry>> FetchMacJavaAsync() {
        var javaEntries = new List<JavaEntry>();

        var directories = Directory.EnumerateDirectories(_macJavaHomePath);
        var tasks = directories.Select(async dir => {
            if (!Directory.Exists(dir + "/Contents/Home/bin"))
                return;

            if (File.Exists($"{dir}/Contents/Home/bin/java")) {
                var javaInfo = await Task.Run(() => JavaUtil.GetJavaInfo($"{dir}/Contents/Home/bin/java"));
                javaEntries.Add(javaInfo);
            }
        });

        await Task.WhenAll(tasks);
        return javaEntries.ToImmutableArray();
    }

    [SupportedOSPlatform(nameof(OSPlatform.Linux))]
    private async ValueTask<ImmutableArray<JavaEntry>> FetchLinuxJavaAsync() {
        var javaEntries = new List<JavaEntry>();

        var tasks = _linuxJavaHomePaths.Select(async LinuxJavaHomePath => {
            if (!Directory.Exists(LinuxJavaHomePath)) {
                return;
            }

            var jvmPaths = Directory.EnumerateDirectories(LinuxJavaHomePath);
            var jvmTasks = jvmPaths.Select(async jvmPath => {
                if (File.Exists($"{jvmPath}/bin/java")) {
                    var javaInfo = await Task.Run(() => JavaUtil.GetJavaInfo($"{jvmPath}/bin/java"));
                    javaEntries.Add(javaInfo);
                }
            });

            await Task.WhenAll(jvmTasks);
        });

        await Task.WhenAll(tasks);
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
        var envJvmPath = await cmd.StandardError.ReadToEndAsync();

        cmd.Close();
        if (File.Exists(envJvmPath)) {
            var javaInfo = await Task.Run(() => JavaUtil.GetJavaInfo(envJvmPath));
            javaEntries.Add(javaInfo);
        }

        return javaEntries.ToImmutableArray();
    }

    [SupportedOSPlatform(nameof(OSPlatform.Windows))]
    private ImmutableArray<JavaEntry> FetchWindowJava() {
        List<string> paths = new();
        string environmentVariable = Environment.GetEnvironmentVariable("Path");
        string[] array = environmentVariable!.Split(Path.PathSeparator);
        foreach (string path in array) {
            string temp = Path.Combine(path, "javaw.exe");
            if (File.Exists(temp)) {
                paths.Add(temp);
            }
        }

        var drives = DriveInfo.GetDrives();
        foreach (var drive in drives) {
            if (!drive.IsReady) {
                continue;
            }

            FetchJavaw(drive.Name.ToDirectoryInfo(), ref paths);
        }

        FetchJavaw(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
            .ToDirectoryInfo(), ref paths);

        FetchJavaw(AppDomain.CurrentDomain.SetupInformation.ApplicationBase
            .ToDirectoryInfo(), ref paths);

        paths.Sort((string x, string s) => x.CompareTo(s));

        return paths.AsParallel()
            .Where(x => !x.Contains("javapath_target_"))
            .Select(JavaUtil.GetJavaInfo)
            .ToImmutableArray();
    }

    [SupportedOSPlatform(nameof(OSPlatform.Windows))]
    private void FetchJavaw(DirectoryInfo directory, ref List<string> results) {
        var javaw = Path.Combine(directory.FullName, "javaw.exe");
        if (File.Exists(javaw)) {
            results.Add(javaw);
        }

        try {
            foreach (DirectoryInfo item in directory.EnumerateDirectories()) {
                if (!item.Attributes.HasFlag(FileAttributes.ReparsePoint)) {
                    string name = item.Name.ToLower();
                    if (item.Parent!.Name.ToLower() == "users" || _windowJavaSearchTerms.Any(name.Contains)) {
                        FetchJavaw(item, ref results);
                    }
                }
            }
        } catch (UnauthorizedAccessException) {
        }
    }
}