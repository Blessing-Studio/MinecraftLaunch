using MinecraftLaunch.Classes.Models.Game;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace MinecraftLaunch.Utilities {
    public class JavaUtil {
        public static JavaEntry GetJavaInfo(string path) {
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) {
                return null;
            }

            try {
                if (!EnvironmentUtil.IsWindow) {
                    using var program = new Process {
                        StartInfo = new ProcessStartInfo {
                            Arguments = "-version",
                            FileName = path,
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            RedirectStandardError = true,
                            RedirectStandardOutput = true
                        }
                    };

                    program.Start();

                    bool is64 = true;
                    string versionInfo = string.Empty;
                    using var reader = program.StandardError;
                    string pattern = "java version \"\\s*(?<version>\\S+)\\s*\"";
                    while (reader.Peek() != -1) {
                        string temp = reader.ReadLine()!;
                        if (temp.Contains("java version")) {
                            versionInfo = new Regex(pattern).Match(temp).Groups["version"].Value;
                        } else if (temp.Contains("openjdk version")) {
                            pattern = pattern.Replace("java", "openjdk");
                            versionInfo = new Regex(pattern).Match(temp).Groups["version"].Value;
                        }

                        is64 = temp.Contains("64-Bit");
                    }

                    string[] versionParts = versionInfo.Split(".");
                    return new JavaEntry {
                        Is64Bit = is64,
                        JavaPath = path,
                        JavaVersion = versionInfo,
                        JavaDirectoryPath = Directory.GetParent(path).FullName,
                        JavaSlugVersion = (int.Parse(versionParts[0]) == 1) ? int.Parse(versionParts[1]) : int.Parse(versionParts[0]),
                    };
                } else {
                    var fileVersionInfo = FileVersionInfo.GetVersionInfo(path);
                    return new JavaEntry {
                        JavaPath = path,
                        Is64Bit = GetIs64Bit(path)!,
                        JavaVersion = fileVersionInfo.ProductVersion!,
                        JavaSlugVersion = fileVersionInfo.ProductMajorPart,
                        JavaDirectoryPath = Directory.GetParent(path).FullName,
                    };
                }
            }
            catch (Exception) {
                return null;
            }
        }

        [SupportedOSPlatform(nameof(OSPlatform.Windows))]
        private static bool GetIs64Bit(string path) {
            ushort architecture = 0;

            try {
                using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
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
}
