using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Flurl;
using Flurl.Http;
using MinecraftLaunch.Modules.Downloaders;
using MinecraftLaunch.Modules.Enum;
using MinecraftLaunch.Modules.Installer;
using MinecraftLaunch.Modules.Interface;
using MinecraftLaunch.Modules.Models.Download;
using MinecraftLaunch.Modules.Models.Install;
using MinecraftLaunch.Modules.Models.Launch;
using MinecraftLaunch.Modules.Utilities;

namespace MinecraftLaunch.Modules.Installer {
    public class JavaInstaller : InstallerBase<JavaInstallerResponse> {
        private double count { get; set; }

        private double allCount { get; set; }

        public string DownloadPath { get; set; }

        public DownloadJavaInfo JavaInfo { get; set; }

        public JavaInstaller(string path, DownloadJavaInfo javaInfo) {
            DownloadPath = Path.Combine(path, javaInfo.DetailVersion);
            JavaInfo = javaInfo;
        }

        public static async IAsyncEnumerable<DownloadJavaInfo> GetJavasByVersionAsync(int javaVersion) {
            string api = $"{(APIManager.Current == APIManager.Mojang ? "https://piston-meta.mojang.com" : APIManager.Current.Host)}" +
                $"/v1/products/java-runtime/2ec0cc96c44e5a76b9c8b7c39df7210883d12871/all.json";

            var nodes = JsonNode.Parse(await api.GetStringAsync())!
                .AsObject();

            var platformName = EnvironmentUtil.GetPlatformName(true);
            var javas = nodes.Where(x => x.Key.Contains(platformName));

            foreach (var java in javas) {
                foreach (var item in java.Value!.AsObject()) {
                    var infoList = item.Value?.AsArray();

                    if (infoList!.Count > 0) {
                        var info = infoList[0];
                        var versionNode = info["version"];
                        string version = versionNode!["name"].GetValue<string>();
                        int firstVersion = version.Contains('.')
                            ? Convert.ToInt32(version.Split('.', 2)[0])
                            : Convert.ToInt32(version[0].ToString());

                        if (firstVersion == javaVersion) {
                            var manifest = info["manifest"];

                            yield return new DownloadJavaInfo {
                                Type = java.Key,
                                Version = firstVersion,
                                DetailVersion = version,
                                Url = manifest["url"].GetValue<string>(),
                            };
                        }
                    }
                }
            }
        }

        public override async ValueTask<JavaInstallerResponse> InstallAsync() {
            try {
                InvokeStatusChangedEvent(0.1f, "开始获取所需的文件列表");
                string baseApi = $"{(APIManager.Current.Host == APIManager.Mojang.Host ? "https://piston-meta.mojang.com" : APIManager.Current.Host)}";
                string api = JavaInfo.Url.Replace("https://piston-meta.mojang.com",baseApi);
                
                string javafileJson = await api.GetStringAsync();
                var javaFiles = JsonNode.Parse(javafileJson)!["files"]!
                    .AsObject()
                    .AsEnumerable();
                allCount = javaFiles.Count();
                InvokeStatusChangedEvent(0.15f, "开始解析文件列表");

                TransformManyBlock<IEnumerable<KeyValuePair<string?, JsonNode?>>,
                    KeyValuePair<string?, JsonNode?>> javafilesHandler = new(x => x);

                ActionBlock<KeyValuePair<string?, JsonNode?>> parallelDownloader = new(async x => {
                    var type = x.Value!["type"]!.GetValue<string>();
                    if (type is "file") {
                        FileInfo path = new(Path.Combine(DownloadPath, x.Key!));
                        string downloadUrl = x.Value["downloads"]!["raw"]!["url"]!
                            .GetValue<string>()
                            .Replace("https://piston-data.mojang.com", baseApi);

                        if (!path.Exists) {
                            var result = await FileDownloader.DownloadAsync(new() {
                                Url = downloadUrl,
                                FileName = path.Name,
                                Directory = path.Directory,
                            });

                            if (result.HttpStatusCode is HttpStatusCode.OK) {
                                count++;
                                var progress = count / allCount;
                                InvokeStatusChangedEvent(0.2f + (float)progress * 0.8f, $"下载中：{allCount}/{count}");
                            }
                        } else {
                            count++;
                            var progress = count / allCount;
                            InvokeStatusChangedEvent(0.2f + (float)progress * 0.8f, $"下载中：{allCount}/{count}");
                        }
                    } else {
                        count++;
                        Directory.CreateDirectory(Path.Combine(DownloadPath,x.Key!));
                    }
                }, new() {
                    BoundedCapacity = 32,
                    MaxDegreeOfParallelism = 32
                });

                DataflowLinkOptions linkOptions = new DataflowLinkOptions {
                    PropagateCompletion = true
                };

                javafilesHandler.LinkTo(parallelDownloader, linkOptions);
                javafilesHandler!.Post(javaFiles);
                javafilesHandler.Complete();

                InvokeStatusChangedEvent(0.2f, "开始下载所有文件");
                await parallelDownloader.Completion;
                InvokeStatusChangedEvent(1f, "安装完成");

                var javaName = EnvironmentUtil.IsWindow ? "javaw.exe" : "java";
                return new() {
                    Success = true,
                    Exception = null,
                    JavaInfo = JavaUtil.GetJavaInfo(new DirectoryInfo(DownloadPath).Find(javaName).FullName),
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

    public record DownloadJavaInfo {
        public string Url { get; set; }

        public int Version { get; set; }

        public string Type { get; set; }

        public string JavaPath { get; set; }

        public string DetailVersion { get; set; }
    }
}
