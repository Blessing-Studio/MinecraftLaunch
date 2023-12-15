﻿using Flurl.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using MinecraftLaunch.Extensions;
using MinecraftLaunch.Components.Resolver;
using MinecraftLaunch.Classes.Models.Game;
using MinecraftLaunch.Classes.Models.Install;
using MinecraftLaunch.Classes.Models.Download;

namespace MinecraftLaunch.Components.Installer {
    public class FabricInstaller(GameEntry inheritedFrom, FabricBuildEntry entry, string customId = default, MirrorDownloadSource source = default) : InstallerBase {
        private readonly string _customId = customId;   
        private FabricBuildEntry _fabricBuildEntry = entry;
        private readonly GameEntry _inheritedFrom = inheritedFrom;

        public override async ValueTask<bool> InstallAsync() {
            /*
             * Parse build
             */
            ReportProgress(0.0d, "Start parse build", TaskStatus.Created);
            string url = $"https://meta.fabricmc.net/v2/versions/loader/{_fabricBuildEntry.McVersion}/{_fabricBuildEntry.BuildVersion}/profile/json";
            var versionInfoNode = JsonNode.Parse(await url.GetStringAsync());
            var libraries = LibrariesResolver.GetLibrariesFromJsonArray(versionInfoNode
                .GetEnumerable("libraries"),
                _inheritedFrom.GameFolderPath);

            /*
             * Download dependent resources
             */
            ReportProgress(0.25d, "Start downloading dependent resources", TaskStatus.WaitingToRun);
            await libraries.DownloadResourceEntrysAsync(source, x => {
                ReportProgress(x.ToPercentage().ToPercentage(0.25d, 0.75d), $"Downloading dependent resources：{x.CompletedCount}/{x.TotalCount}",
                    TaskStatus.Running);
            });

            /*
             * Write information to version json
             */
            ReportProgress(0.85d, "Write information to version json", TaskStatus.WaitingToRun);
            if (!string.IsNullOrEmpty(_customId)) {
                versionInfoNode = versionInfoNode.SetString("id", _customId);
            }

            var id = versionInfoNode.GetString("id");
            var jsonFile = new FileInfo(Path.Combine(_inheritedFrom.GameFolderPath,
                "versions", id, $"{id}.json"));

            if (!jsonFile.Directory.Exists) {
                jsonFile.Directory.Create();
            }

            File.WriteAllText(jsonFile.FullName, versionInfoNode.ToString());
            ReportProgress(1.0d, "Installation is complete", TaskStatus.Canceled);
            return true;
        }

        public static async ValueTask<IEnumerable<FabricBuildEntry>> EnumerableFromVersionAsync(string mcVersion) {
            string url = $"https://meta.fabricmc.net/v2/versions/loader/{mcVersion}";
            string json = await url.GetStringAsync();

            var entries = JsonSerializer.Deserialize<List<FabricBuildEntry>>(json);

            entries = entries
                .OrderByDescending(entry =>
                    new Version(entry.Loader.Version.Replace(entry.Loader.Separator, "."))
                ).ToList();

            return entries;
        }
    }
}
