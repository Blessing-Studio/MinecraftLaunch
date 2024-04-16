using System.Text.Json;
using MinecraftLaunch.Extensions;
using MinecraftLaunch.Classes.Interfaces;
using MinecraftLaunch.Classes.Models.Game;

namespace MinecraftLaunch.Components.Resolver;

/// <summary>
/// Minecraft 资源材质库解析器
/// </summary>
internal sealed class AssetsResolver(GameEntry entity) {
    public AssetEntry GetAssetIndexJson() {
        var assetIndex = (File.ReadAllText(entity.IsInheritedFrom 
                ? entity.InheritsFrom.OfVersionJsonPath() 
                : entity.OfVersionJsonPath()))
            .AsNode()
            .Select("assetIndex")
            .Deserialize<AssstIndex>();

        var assetIndexFilePath = entity.IsInheritedFrom
            ? entity.InheritsFrom.AssetsIndexJsonPath 
            : entity.AssetsIndexJsonPath;

        if(assetIndexFilePath is null) {
            return default!;
        }

        return new AssetEntry {
            Url = assetIndex!.Url,
            Path = assetIndexFilePath,
            Checksum = assetIndex.Sha1,
            Name = assetIndex.Id + ".json",
            RelativePath = assetIndexFilePath.Replace(Path.Combine(entity.GameFolderPath, "assets"), string.Empty).TrimStart('\\')
        };
    }

    public IEnumerable<AssetEntry> GetAssets() {
        if (string.IsNullOrEmpty(entity.AssetsIndexJsonPath))
            yield break;

        var assets = File.ReadAllText(entity.AssetsIndexJsonPath).AsNode()
            .Select("objects")
            .Deserialize<Dictionary<string, AssetJsonEntry>>();

        foreach (var keyValuePair in assets!) {
            yield return Resolve(keyValuePair);
        }
    }

    public AssetEntry Resolve(KeyValuePair<string, AssetJsonEntry> parameter) {
        var hashPath = Path.Combine(parameter.Value.Hash[..2], parameter.Value.Hash);
        var relativePath = Path.Combine("objects", hashPath);
        var absolutePath = Path.Combine(entity.GameFolderPath, "assets", relativePath);

        return new AssetEntry {
            Path = absolutePath,
            Name = parameter.Key,
            Size = parameter.Value.Size,
            RelativePath = relativePath,
            Checksum = parameter.Value.Hash,
            Url = "https://resources.download.minecraft.net/" + hashPath.Replace('\\', '/')
        };
    }
}