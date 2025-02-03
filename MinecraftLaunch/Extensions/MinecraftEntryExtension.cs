using MinecraftLaunch.Base.Models.Game;
using MinecraftLaunch.Base.Utilities;
using System.IO.Compression;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace MinecraftLaunch.Extensions;

public static class MinecraftEntryExtension {
    public static MinecraftClient GetJarElement(this MinecraftEntry entry) {
        string clientJsonPath = entry.ClientJsonPath;
        if (entry is ModifiedMinecraftEntry { HasInheritance: true } inst)
            clientJsonPath = inst.InheritedMinecraftEntry.ClientJsonPath;

        JsonNode clientArtifactNode = File.ReadAllText(clientJsonPath).AsNode().Select("downloads")?.Select("client");

        if (clientArtifactNode is null)
            return null;

        string clientJarPath = entry.ClientJarPath;
        if (entry is ModifiedMinecraftEntry { HasInheritance: true } inst_)
            clientJarPath = inst_.ClientJarPath;

        if (clientJarPath is null)
            return null;

        int? size = clientArtifactNode.GetInt32("size");
        string url = clientArtifactNode.GetString("url");
        string sha1 = clientArtifactNode.GetString("sha1");

        if (sha1 is null || url is null || size is null)
            throw new InvalidDataException("Invalid client info");

        return new MinecraftClient {
            MinecraftFolderPath = entry.MinecraftFolderPath,
            ClientId = Path.GetFileNameWithoutExtension(clientJarPath),
            Url = url,
            Size = (int)size,
            Sha1 = sha1
        };
    }

    public static AssstIndex GetAssetIndex(this MinecraftEntry minecraftEntry) {
        // Identify file paths
        string clientJsonPath = minecraftEntry is ModifiedMinecraftEntry { HasInheritance: true } entry
            ? entry.InheritedMinecraftEntry.ClientJsonPath
            : minecraftEntry.ClientJsonPath;

        // Parse client.json
        JsonNode jsonNode = JsonNode.Parse(File.ReadAllText(clientJsonPath));
        var assetIndex = jsonNode.Select("assetIndex")
            ?? throw new InvalidDataException("Error in parsing version.json");

        // TODO: Handle nullable check in Json deserialization (requires .NET 9)
        string id = assetIndex.GetString("id") ?? throw new InvalidDataException();
        string url = assetIndex.GetString("url") ?? throw new InvalidDataException();
        string sha1 = assetIndex.GetString("sha1") ?? throw new InvalidDataException();

        return new AssstIndex {
            Id = id,
            Url = url,
            Sha1 = sha1,
            MinecraftFolderPath = minecraftEntry.MinecraftFolderPath,
        };
    }

    public static void ExtractNatives(this MinecraftEntry minecraftEntry, IReadOnlyList<MinecraftLibrary> natives) {
        if (!natives.Any()) return;

        var extension = EnvironmentUtil.GetPlatformName() switch {
            "windows" => ".dll",
            "linux" => ".so",
            "osx" => ".dylib",
            _ => "."
        };

        foreach (var file in natives) {
            using ZipArchive zip = ZipFile.OpenRead(file.FullPath);

            foreach (ZipArchiveEntry entry in zip.Entries) {
                if (Path.HasExtension(entry.FullName)) {
                    var toExtract = new FileInfo(Path.Combine(minecraftEntry.MinecraftFolderPath, "versions", minecraftEntry.Id, "natives", entry.Name));
                    toExtract.Directory?.Create();
                    if (!toExtract.Exists) {
                        entry.ExtractToFile(toExtract.FullName, true);
                    }
                }
            }
        }
    }

    public static Task ExtractNativesAsync(this MinecraftEntry minecraftEntry, IReadOnlyList<MinecraftLibrary> natives, CancellationToken cancellationToken = default) => Task.Run(() => {
        if (!natives.Any()) return;

        var extension = EnvironmentUtil.GetPlatformName() switch {
            "windows" => ".dll",
            "linux" => ".so",
            "osx" => ".dylib",
            _ => "."
        };

        foreach (var file in natives) {
            using ZipArchive zip = ZipFile.OpenRead(file.FullPath);

            foreach (ZipArchiveEntry entry in zip.Entries) {
                if (Path.HasExtension(entry.FullName)) {
                    var toExtract = new FileInfo(Path.Combine(minecraftEntry.MinecraftFolderPath, "versions", minecraftEntry.Id, "natives", entry.Name));
                    toExtract.Directory?.Create();
                    if (!toExtract.Exists) {
                        entry.ExtractToFile(toExtract.FullName, true);
                    }
                }
            }
        }
    }, cancellationToken);
}