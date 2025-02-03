using MinecraftLaunch.Base.Enums;
using MinecraftLaunch.Base.EventArgs;
using MinecraftLaunch.Base.Interfaces;
using MinecraftLaunch.Base.Models.Game;
using MinecraftLaunch.Base.Models.Network;
using MinecraftLaunch.Extensions;
using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace MinecraftLaunch.Components.Downloader;

public sealed class MinecraftResourceDownloader {
    private readonly MinecraftEntry _entry;
    private readonly FileDownloader _downloader;
    private readonly List<MinecraftDependency> _dependencies = [];

    public event EventHandler<ResourceDownloadProgressChangedEventArgs> ProgressChanged;

    public bool AllowVerifyAssets { get; init; } = true;
    public bool AllowInheritedDependencies { get; init; } = true;

    public MinecraftResourceDownloader(MinecraftEntry entry, int maxThread = 64, IEnumerable<MinecraftDependency> extraDependencies = null) {
        if (extraDependencies is not null)
            _dependencies.AddRange(extraDependencies);

        _entry = entry;
        _downloader = new(maxThread);
    }

    public async Task<GroupDownloadResult> VerifyAndDownloadDependenciesAsync(int fileVerificationParallelism = 10, CancellationToken cancellationToken = default) {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(fileVerificationParallelism);

        #region 1.1 Libraries & Inherited Libraries

        var (libs, nativeLibs) = _entry.GetRequiredLibraries();
        _dependencies.AddRange(libs);
        _dependencies.AddRange(nativeLibs);

        if (AllowInheritedDependencies
            && _entry is ModifiedMinecraftEntry modInstance
            && modInstance.HasInheritance) {
            (libs, nativeLibs) = modInstance.InheritedMinecraftEntry.GetRequiredLibraries();
            _dependencies.AddRange(libs);
            _dependencies.AddRange(nativeLibs);
        }

        #endregion

        #region 1.2 Client.jar

        var jar = _entry.GetJarElement();
        if (jar != null) {
            _dependencies.Add(jar);
        }

        #endregion

        #region 1.3 AssetIndex & Assets

        if (AllowVerifyAssets) {
            var assetIndex = _entry.GetAssetIndex();

            if (!await VerifyDependencyAsync(assetIndex, cancellationToken)) {
                var result = await _downloader
                    .DownloadFileAsync(new(assetIndex.Url, assetIndex.FullPath), cancellationToken);

                if (result.Type == DownloadResultType.Failed) {
                    throw new Exception("Failed to obtain the dependent material index file");
                }
            }

            _dependencies.AddRange(_entry.GetRequiredAssets());
        }

        #endregion

        // 2. Verify dependencies
        SemaphoreSlim semaphore = new(fileVerificationParallelism, fileVerificationParallelism);
        ConcurrentBag<MinecraftDependency> invalidDeps = [];

        var tasks = _dependencies.Select(async dep => {
            await semaphore.WaitAsync(cancellationToken);
            try {
                if (!await VerifyDependencyAsync(dep, cancellationToken)) {
                    lock (invalidDeps) {
                        invalidDeps.Add(dep);
                    }
                }
            } finally {
                semaphore.Release();
            }
        }).ToList();

        await Task.WhenAll(tasks);
        //InvalidDependenciesDetermined?.Invoke(this, invalidDeps);
        
        // 3. Download invalid dependencies
        var downloadItems = invalidDeps.Where(dep => dep is IDownloadDependency)
            .OfType<IDownloadDependency>()
            .Select(dep => new DownloadRequest(dep.Url, dep.FullPath));

        int currentCount = 0;
        double speed = default;
        int totalCount = downloadItems.Count();
        var groupRequest = new GroupDownloadRequest(downloadItems);
        groupRequest.DownloadSpeedChanged += arg => speed = arg;

        groupRequest.SingleRequestCompleted += (request, result) => {
            Interlocked.Increment(ref currentCount);
            ProgressChanged?.Invoke(this, new ResourceDownloadProgressChangedEventArgs {
                Speed = speed,
                TotalCount = totalCount,
                CompletedCount = currentCount,
            });
        };

        return await _downloader.DownloadFilesAsync(groupRequest, cancellationToken);
    }

    #region Privates

    private static async Task<bool> VerifyDependencyAsync(MinecraftDependency dep, CancellationToken cancellationToken = default) {
        if (!File.Exists(dep.FullPath))
            return false;

        if (dep is not IVerifiableDependency verifiableDependency)
            return true;

        async Task<bool> VerifySha1Async(CancellationToken cancellationToken) {
            using var fileStream = File.OpenRead(dep.FullPath);
            byte[] sha1Bytes = await SHA1.HashDataAsync(fileStream, cancellationToken);
            string sha1Str = BitConverter.ToString(sha1Bytes).Replace("-", string.Empty).ToLower();

            return sha1Str == verifiableDependency.Sha1;
        }

        async Task<bool> VerifySizeAsync(CancellationToken _) {
            var file = new FileInfo(dep.FullPath);
            return await Task.FromResult(verifiableDependency.Size == file.Length);
        }

        if (verifiableDependency.Sha1 != null)
            return await VerifySha1Async(cancellationToken);
        else if (verifiableDependency.Size != null)
            return await VerifySizeAsync(cancellationToken);

        return true;
    }

    #endregion
}