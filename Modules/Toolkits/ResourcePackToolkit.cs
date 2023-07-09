using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MinecraftLaunch.Modules.Interface;
using MinecraftLaunch.Modules.Models.Download;
using MinecraftLaunch.Modules.Models.Launch;

namespace MinecraftLaunch.Modules.Toolkits;

public class ResourcePackToolkit : IPackToolkit<ResourcePack> {
    public static readonly string X = (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "\\" : "/");

    private static GameCore? GameCore = null;

    private static bool IsNewOptionFormat = false;

    private static bool IsCopy;

    private static bool IsEnabled;

    private static bool IsOlate;

    private static string ResourcePacksDirectory = string.Empty;

    private static string WorkingDirectory = string.Empty;

    public async ValueTask<ImmutableArray<ResourcePack>> LoadAllAsync() {
        string optionsFile = (WorkingDirectory.EndsWith('/') ? WorkingDirectory : Path.Combine(WorkingDirectory, "options.txt"));

        string?[] enabledPacksIds = null;
        if (File.Exists(optionsFile)) {
            using StreamReader reader = new StreamReader(optionsFile, Encoding.Default);
            string line;
            while ((line = reader.ReadLine()) != null) {
                if (!line.StartsWith("resourcePacks")) {
                    continue;
                }
                string text = line;
                enabledPacksIds = (from id in text.Substring(15, text.Length - 1 - 15).Split(',').Select(delegate (string id)
                {
                    if (string.IsNullOrWhiteSpace(id)) {
                        return null;
                    }
                    string text2 = id;
                    id = text2.Substring(1, text2.Length - 1 - 1);
                    if (id == "vanilla") {
                        IsNewOptionFormat = true;
                    }
                    if (id.StartsWith("file/")) {
                        text2 = id;
                        id = text2.Substring(5, text2.Length - 5);
                    }
                    return id;
                })
                                   where id != null
                                   select id).ToArray();
                break;
            }
        }
        Directory.CreateDirectory(ResourcePacksDirectory);
        ILookup<bool, ResourcePack> obj = await Task.FromResult((from path in Directory.EnumerateFileSystemEntries(ResourcePacksDirectory)
                                                                 select LoadSingle(path, enabledPacksIds) into pack
                                                                 where pack != null
                                                                 select pack).ToLookup((ResourcePack pack) => pack.IsEnabled));
        IOrderedEnumerable<ResourcePack> enabledPacks = obj[true].OrderByDescending((ResourcePack pack) => Array.IndexOf<string>(enabledPacksIds, pack.Id));
        IOrderedEnumerable<ResourcePack> disabledPacks = obj[false].OrderBy((ResourcePack pack) => pack.Id);
        return enabledPacks.Concat(disabledPacks).ToImmutableArray();
    }

    public async ValueTask<ImmutableArray<ResourcePack>> MoveLoadAllAsync(IEnumerable<string> Paths) {
        return await ValueTask.FromResult((from pack in Paths.Select(delegate (string path)
        {
            string text = Path.Combine(ResourcePacksDirectory, Path.GetFileName(path));
            if (File.Exists(text)) {
                return null;
            }
            ResourcePack resourcePack = LoadSingle(path);
            if (resourcePack == null) {
                return null;
            }
            try {
                if (IsCopy) {
                    File.Copy(path, text);
                } else {
                    File.Move(path, text);
                }
            }
            catch (IOException) {
                return null;
            }
            resourcePack.Path = text;
            resourcePack.IsEnabled = IsEnabled;
            return resourcePack;
        })
                                           where pack != null
                                           select pack).ToImmutableArray());
    }

    public ResourcePack LoadSingle(string path, string[] enabledPacksIds = null) {
        string id = Path.GetFileName(path);
        bool isZip = path.EndsWith(".zip");
        MemoryStream infoMemStream = new MemoryStream();
        MemoryStream imgMemStream = new MemoryStream();
        if (isZip) {
            using ZipArchive archive = ZipFile.OpenRead(path);
            ZipArchiveEntry infoEntry = archive.GetEntry("pack.mcmeta");
            if (infoEntry == null) {
                return null;
            }
            Stream stream = infoEntry.Open();
            stream.CopyTo(infoMemStream);
            ZipArchiveEntry imgEntry = archive.GetEntry("pack.png");
            if (imgEntry != null) {
                Stream stream2 = imgEntry.Open();
                stream2.CopyTo(imgMemStream);
            }
        } else {
            string infoFile = Path.Combine(path, "pack.mcmeta");
            if (!File.Exists(infoFile)) {
                return null;
            }
            FileStream infoStream = File.OpenRead(infoFile);
            infoStream.CopyTo(infoMemStream);
            string imgFile = Path.Combine(path, "pack.png");
            if (File.Exists(imgFile)) {
                FileStream imgStream = File.OpenRead(imgFile);
                imgStream.CopyTo(imgMemStream);
            }
        }
        try {
            ResourcePackInfo info = JsonSerializer.Deserialize<ResourcePackInfo>(CryptoToolkit.Remove(infoMemStream.ToArray(), 1));
            return new ResourcePack {
                Id = id,
                Path = path,
                IsEnabled = (enabledPacksIds?.Contains(id) ?? false),
                IsExtracted = !isZip,
                Description = Regex.Replace(info.pack?.description ?? string.Empty, "§.", string.Empty),
                ImageStream = imgMemStream
            };
        }
        catch {
        }
        return null;
    }

    public bool EnabledResourcePacks(IEnumerable<ResourcePack> enabledPacks) {
        string optionsPath = (WorkingDirectory.EndsWith('/') ? WorkingDirectory : Path.Combine(WorkingDirectory, "options.txt"));
        string options = ((!File.Exists(optionsPath)) ? "resourcePacks:[]" : File.ReadAllText(optionsPath, Encoding.Default));
        string enabledPackIDs = string.Join(",", from pack in enabledPacks.Reverse()
                                                 select (!IsNewOptionFormat) ? ("\"" + pack.Id + "\"") : ("\"file/" + pack.Id + "\""));
        options = ((!options.Contains("resourcePacks:[")) ? (options + "resourcePacks:[" + enabledPackIDs + "]\r\n") : Regex.Replace(options, "resourcePacks:\\[.*\\]", "resourcePacks:[" + enabledPackIDs + "]"));
        File.WriteAllText(optionsPath, options, Encoding.Default);
        return true;
    }

    public ResourcePackToolkit(GameCore? Id, bool isEnabled, bool Isolate = true) {
        GameCore = Id;
        IsCopy = true;
        IsOlate = Isolate;
        IsEnabled = isEnabled;
        ResourcePacksDirectory = Id.GetResourcePacksPath(Isolate);
        WorkingDirectory = Id.GetGameCorePath();
    }
}
