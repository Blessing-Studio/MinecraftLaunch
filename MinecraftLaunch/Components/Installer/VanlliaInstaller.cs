﻿using Flurl.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using MinecraftLaunch.Extensions;
using MinecraftLaunch.Classes.Interfaces;
using MinecraftLaunch.Components.Checker;
using MinecraftLaunch.Classes.Models.Install;
using MinecraftLaunch.Classes.Models.Download;

namespace MinecraftLaunch.Components.Installer {
    /// <summary>
    /// 原版核心安装器
    /// </summary>
    public class VanlliaInstaller(IGameResolver gameFoloder, string gameId, MirrorDownloadSource source = default) : InstallerBase {
        private string _gameId = gameId;

        private ResourceChecker _resourceChecker;

        private MirrorDownloadSource _source = source;

        private IGameResolver _gameResolver = gameFoloder;

        public override async ValueTask<bool> InstallAsync() {
            /*
             * Check if the specified id exists
             */
            ReportProgress(0.0d, "Check if the specified id exists", TaskStatus.Created);
            var cache = await EnumerableGameCoreAsync(source);
            if (cache is null || string.IsNullOrEmpty(_gameId)) {
                return false;
            }

            /*
             * Download game core json
             */
            ReportProgress(0.15d, "Start downloading the game core json", TaskStatus.WaitingToRun);
            var coreInfo = cache.SingleOrDefault(x => x.Id == _gameId);
            if (coreInfo is null) {
                return false;
            }

            var versionJsonFile = Path.Combine(_gameResolver.Root.FullName, "versions", _gameId,
                $"{_gameId}.json").ToFileInfo();

            if (!versionJsonFile.Directory.Exists) {
                versionJsonFile.Directory.Create();
            }

            await File.WriteAllTextAsync(versionJsonFile.FullName, 
                await coreInfo.Url.GetStringAsync());

            /*
             * Download dependent resources
             */
            ReportProgress(0.45d, "Start downloading dependent resources", TaskStatus.WaitingToRun);
            _resourceChecker = new(_gameResolver.GetGameEntity(_gameId));
            await _resourceChecker.CheckAsync();

            await _resourceChecker.MissingResources.DownloadResourceEntrysAsync(source,
                x => {
                    ReportProgress(x.ToPercentage().ToPercentage(0.45d, 0.95d),
                        $"Downloading dependent resources：{x.CompletedCount}/{x.TotalCount}",
                        TaskStatus.Running);
                });


            ReportProgress(1.0d, "Installation is complete", TaskStatus.Canceled);
            return true;
        }

        public static async ValueTask<IEnumerable<VersionManifestEntry>> EnumerableGameCoreAsync(MirrorDownloadSource source = default) {
            string url = string.Empty;
            if (MirrorDownloadManager.IsUseMirrorDownloadSource && source is not null) {
                url = source.VersionManifestUrl;
            } else {
                url = "http://launchermeta.mojang.com/mc/game/version_manifest.json";
            }

            var node = JsonNode.Parse(await url.GetStringAsync());
            return node.GetEnumerable("versions").Deserialize<IEnumerable<VersionManifestEntry>>();
        }
    }
}
