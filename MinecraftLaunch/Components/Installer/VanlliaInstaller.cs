using Flurl.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using MinecraftLaunch.Extensions;
using System.Collections.Immutable;
using MinecraftLaunch.Classes.Interfaces;
using MinecraftLaunch.Classes.Models.Event;
using MinecraftLaunch.Classes.Models.Install;
using MinecraftLaunch.Classes.Models.Download;
using MinecraftLaunch.Components.Checker;
using MinecraftLaunch.Classes.Models.Game;
using MinecraftLaunch.Components.Resolver;

namespace MinecraftLaunch.Components.Installer {
    /// <summary>
    /// 原版核心安装器
    /// </summary>
    public class VanlliaInstaller(string gameFoloder, string gameId, MirrorDownloadSource source = default) : IInstaller {
        private string _gameId = gameId;

        private ResourceChecker _resourceChecker;

        private string _gameFoloder = gameFoloder;

        private MirrorDownloadSource _source = source;

        private GameResolver _gameResolver = new(gameFoloder);

        private static IEnumerable<VersionManifestEntry> _cache;

        public event EventHandler<EventArgs> Completed;

        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

        public async ValueTask<bool> InstallAsync() {
            /*
             * Check if the specified id exists
             */
            ReportProgress(0.0d, "Check if the specified id exists", TaskStatus.Created);
            if (_cache is null && string.IsNullOrEmpty(_gameId)) {
                return false;
            }

            /*
             * Download game core json
             */
            ReportProgress(0.15d, "Start downloading the game core json", TaskStatus.WaitingToRun);
            var coreInfo = _cache.SingleOrDefault(x => x.Id == _gameId);
            if (coreInfo is null) {
                return false;
            }

            var versionJsonFile = Path.Combine(_gameFoloder, "versions", _gameId,
                $"{_gameId}.json").ToFileInfo();

            if (!versionJsonFile.Directory.Exists) {
                versionJsonFile.Directory.Create();
            }

            await File.WriteAllTextAsync(versionJsonFile.FullName, 
                await coreInfo.Url.GetStringAsync());

            /*
             * Download dependent resources
             */
            ReportProgress(0.15d, "Start downloading dependent resources", TaskStatus.WaitingToRun);
            _resourceChecker = new(_gameResolver.GetGameEntity(_gameId));
            await _resourceChecker.CheckAsync();

            await _resourceChecker.MissingResources.DownloadResourceEntrysAsync(source,
                x => {
                    ReportProgress(0.15d, $"Downloading dependent resources：{x.CompletedCount}/{x.TotalCount}",
                        TaskStatus.Running, x.ToSpeedText());
                });


            ReportProgress(0.15d, "Installation is complete", TaskStatus.Canceled);
            return true;
        }

        public void ReportProgress(double progress, string progressStatus, TaskStatus status) {
            ProgressChanged?.Invoke(this, new(status, progress, progressStatus));
        }

        public void ReportProgress(double progress, string progressStatus, TaskStatus status, string speed) {
            ProgressChanged?.Invoke(this, new(status, progress, progressStatus) {
                Speed = speed
            });
        }

        public static async ValueTask<IEnumerable<VersionManifestEntry>> EnumerableGameCoreAsync(MirrorDownloadSource source = default) {
            string url = string.Empty;
            if (MirrorDownloadManager.IsUseMirrorDownloadSource && source is not null) {
                url = source.VersionManifestUrl;
            } else {
                url = "http://launchermeta.mojang.com/mc/game/version_manifest.json";
            }

            var node = JsonNode.Parse(await url.GetStringAsync());
            return _cache = node.GetEnumerable("versions").Deserialize<IEnumerable<VersionManifestEntry>>();
        }
    }
}
