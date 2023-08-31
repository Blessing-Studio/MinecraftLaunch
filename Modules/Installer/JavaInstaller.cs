using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using MinecraftLaunch.Modules.Enum;
using MinecraftLaunch.Modules.Interface;
using MinecraftLaunch.Modules.Models.Download;
using MinecraftLaunch.Modules.Models.Install;
using MinecraftLaunch.Modules.Utils;

namespace MinecraftLaunch.Modules.Installer {
    public partial class JavaInstaller : InstallerBase<JavaInstallerResponse> {
        public static (string, string, PcType) ActiveJdk;

        public static Dictionary<string, KeyValuePair<string, string>[]> OpenJdkDownloadSourcesForWindows
        {
            get
            {
                Dictionary<string, KeyValuePair<string, string>[]> dictionary = new Dictionary<string, KeyValuePair<string, string>[]>();
                dictionary.Add("OpenJDK 8", new KeyValuePair<string, string>[1]
                {
                new KeyValuePair<string, string>("jdk.java.net", "https://download.java.net/openjdk/jdk8u42/ri/openjdk-8u42-b03-windows-i586-14_jul_2022.zip")
                });
                dictionary.Add("OpenJDK 11", new KeyValuePair<string, string>[2]
                {
                new KeyValuePair<string, string>("jdk.java.net", "https://download.java.net/openjdk/jdk11/ri/openjdk-11+28_windows-x64_bin.zip"),
                new KeyValuePair<string, string>("Microsoft", "https://aka.ms/download-jdk/microsoft-jdk-11.0.16-windows-x64.zip")
                });
                dictionary.Add("OpenJDK 17", new KeyValuePair<string, string>[2]
                {
                new KeyValuePair<string, string>("jdk.java.net", "https://download.java.net/openjdk/jdk17/ri/openjdk-17+35_windows-x64_bin.zip"),
                new KeyValuePair<string, string>("Microsoft", "https://aka.ms/download-jdk/microsoft-jdk-17.0.4-windows-x64.zip")
                });
                dictionary.Add("OpenJDK 18", new KeyValuePair<string, string>[1]
                {
                new KeyValuePair<string, string>("jdk.java.net", "https://download.java.net/openjdk/jdk18/ri/openjdk-18+36_windows-x64_bin.zip")
                });
                return dictionary;
            }
        }

        public static Dictionary<string, KeyValuePair<string, string>[]> OpenJdkDownloadSourcesForLinux
        {
            get
            {
                Dictionary<string, KeyValuePair<string, string>[]> dictionary = new Dictionary<string, KeyValuePair<string, string>[]>();
                dictionary.Add("OpenJDK 8", new KeyValuePair<string, string>[1]
                {
                new KeyValuePair<string, string>("Eclipse Adoptium", "https://objects.githubusercontent.com/github-production-release-asset-2e65be/372924428/3464d2a7-e197-4f0a-a70f-a5a7829aa938?X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Credential=AKIAIWNJYAX4CSVEH53A%2F20230126%2Fus-east-1%2Fs3%2Faws4_request&X-Amz-Date=20230126T141228Z&X-Amz-Expires=300&X-Amz-Signature=fdee395b03a54cde693c64b9e50fced1f499e29d0654d8abd1c0eb2e8b074bb8&X-Amz-SignedHeaders=host&actor_id=93388692&key_id=0&repo_id=372924428&response-content-disposition=attachment%3B%20filename%3DOpenJDK8U-jdk_x64_linux_hotspot_8u362b09.tar.gz&response-content-type=application%2Foctet-stream")
                });
                dictionary.Add("OpenJDK 11", new KeyValuePair<string, string>[1]
                {
                new KeyValuePair<string, string>("Microsoft", "https://aka.ms/download-jdk/microsoft-jdk-11.0.18-linux-x64.tar.gz")
                });
                dictionary.Add("OpenJDK 17", new KeyValuePair<string, string>[1]
                {
                new KeyValuePair<string, string>("Microsoft", "https://aka.ms/download-jdk/microsoft-jdk-17.0.6-linux-x64.tar.gz")
                });
                dictionary.Add("OpenJDK 18", new KeyValuePair<string, string>[1]
                {
                new KeyValuePair<string, string>("Eclipse Adoptium", "https://objects.githubusercontent.com/github-production-release-asset-2e65be/376085653/931f3369-e981-4704-9019-c520dd4f8250?X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Credential=AKIAIWNJYAX4CSVEH53A%2F20230126%2Fus-east-1%2Fs3%2Faws4_request&X-Amz-Date=20230126T141448Z&X-Amz-Expires=300&X-Amz-Signature=a5deba64e8f2bece531b13e896e6e9ab3e865a724d9d6fafc460b1b8e29e4a63&X-Amz-SignedHeaders=host&actor_id=93388692&key_id=0&repo_id=376085653&response-content-disposition=attachment%3B%20filename%3DOpenJDK18U-jdk_x64_linux_hotspot_18.0.2.1_1.tar.gz&response-content-type=application%2Foctet-stream")
                });
                return dictionary;
            }
        }

        public static string StorageFolder => ActiveJdk.Item2;

        public override async ValueTask<JavaInstallerResponse> InstallAsync() {
            try {
                string item = ActiveJdk.Item1;
                _ = ActiveJdk;

                InvokeStatusChangedEvent(0.1f, "开始下载 Jdk");
                HttpDownloadResponse res = await HttpUtil.HttpDownloadAsync(item, Path.GetTempPath(), (e, a) => {
                    InvokeStatusChangedEvent(0.1f + e * 0.8f, "下载中：" + a);
                }, null);
                if (res.HttpStatusCode != HttpStatusCode.OK) {
                    return new JavaInstallerResponse {
                        Success = false,
                        Exception = null!,
                        JavaInfo = null!
                    };
                }

                InvokeStatusChangedEvent(0.8f, "开始解压 Jdk");
                await Task.Delay(1000);
                ZipFile.ExtractToDirectory(res.FileInfo.FullName, StorageFolder);
                InvokeStatusChangedEvent(0.95f, "开始删除 下载缓存");
                res.FileInfo.Delete();
                InvokeStatusChangedEvent(1f, "安装完成");
                return new JavaInstallerResponse {
                    Success = true,
                    Exception = null!,
                    JavaInfo = JavaUtil.GetJavaInfo(Path.Combine(Directory.GetDirectories(StorageFolder)[0], "bin"))
                };
            }
            catch (Exception ex) {
                return new JavaInstallerResponse {
                    Success = false,
                    Exception = ex,
                    JavaInfo = null!
                };
            }
        }
    }

    partial class JavaInstaller {
        public JavaInstaller() { }

        public JavaInstaller(JdkDownloadSource jdkDownloadSource, OpenJdkType openJdkType, string SavePath, PcType pcType = PcType.Windows) {
            if (jdkDownloadSource != 0 && jdkDownloadSource != JdkDownloadSource.Microsoft) {
                throw new ArgumentException("选择了错误的下载源");
            }
            if (openJdkType != 0 && openJdkType != OpenJdkType.OpenJdk11 && openJdkType != OpenJdkType.OpenJdk17 && openJdkType != OpenJdkType.OpenJdk18) {
                throw new ArgumentException("选择了错误的Jdk版本");
            }
            if (!Directory.Exists(SavePath)) {
                Directory.CreateDirectory(SavePath);
            }
            ActiveJdk = (openJdkType.ToDownloadLink(jdkDownloadSource), openJdkType.ToFullJavaPath(SavePath), pcType);
        }
    }
}