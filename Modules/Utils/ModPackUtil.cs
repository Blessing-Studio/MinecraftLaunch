using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using MinecraftLaunch.Modules.Interface;
using MinecraftLaunch.Modules.Models.Download;
using MinecraftLaunch.Modules.Models.Launch;
using MinecraftLaunch.Modules.Parser;

namespace MinecraftLaunch.Modules.Utils;

public class ModPackUtil : IPackToolkit<ModPack> {
    private static GameCore? GameCore = null;

    private static bool IsNewOptionFormat = false;

    private static bool IsCopy;

    private static bool IsOlate;

    public static string ModPacksDirectory { get; internal set; } = string.Empty;

    public static string WorkingDirectory { get; internal set; } = string.Empty;

    public async ValueTask<ImmutableArray<ModPack>> LoadAllAsync() {
        List<ModPack> modPacks = new List<ModPack>();
        Directory.CreateDirectory(ModPacksDirectory);
        FileInfo[] files = new DirectoryInfo(ModPacksDirectory).GetFiles();
        foreach (FileInfo file in files) {
            ModPack v = LoadSingle(file.FullName);
            modPacks.Add(v);
        }

        return await Task.FromResult(modPacks.ToImmutableArray());
    }

    public async ValueTask<ImmutableArray<ModPack>> MoveLoadAllAsync(IEnumerable<string> paths) {
        return await Task.FromResult((from mod in paths.Select(delegate (string path) {
            string text = Path.Combine(ModPacksDirectory, Path.GetFileName(path));
            if (File.Exists(text)) {
                return null;
            }

            ModPack modPack = LoadSingle(path);
            if (modPack == null) {
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

            modPack.Path = text;
            return modPack;
        })
                                      where mod != null
                                      select mod).ToImmutableArray());
    }

    public string ModStateChange(string modname) {
        FileInfo file = new FileInfo(modname);
        if (!(Path.GetExtension(modname) == ".disabled")) {
            string newfilepth = file.FullName + ".disabled";
            file.MoveTo(newfilepth);
            return newfilepth;
        }
        string path = Path.ChangeExtension(file.FullName, "");
        file.MoveTo(path.Remove(path.Length - 1, 1));
        return path.Remove(path.Length - 1, 1);
    }

    private ModPack LoadSingle(string path) {
        ModPack mod = null!;
        try {
            using ZipArchive archive = ZipFile.OpenRead(path);
            using Stream fabricModInfo = archive.GetEntry("fabric.mod.json")?.Open()!;
            using Stream forgeModInfo = archive.GetEntry("mcmod.info")?.Open()!;
            using Stream forgeModtoml = archive.GetEntry("META-INF/mods.toml")?.Open()!;

            if (fabricModInfo != null) {
                mod = LoadFabricMod(fabricModInfo);
            } else if (forgeModInfo != null) {
                mod = LoadForgeModByInfo(forgeModInfo);
            }else if (forgeModInfo is null) {
                mod = LoadForgeModByToml(forgeModtoml);
            }
        }
        catch (Exception) {
        }

        if (mod == null) {
            mod = new ModPack();
        }
        
        mod.Path = path;
        mod.FileName = new FileInfo(path).Name;
        mod.IsEnabled = ((!path.EndsWith(".disabled")) ? true : false);
        mod.DisplayName ??= ((!string.IsNullOrWhiteSpace(mod.Description)) ? (mod.Description + ((!string.IsNullOrEmpty(mod.Authors)) ? ("\nby " + mod.Authors) : null)) : mod.Id);
        return mod;
    }

    private static ModPack LoadFabricMod(Stream infoStream) {
        using MemoryStream memoryStream = new MemoryStream();
        infoStream.CopyTo(memoryStream);
        FabricMod fabricMod = JsonSerializer.Deserialize<FabricMod>(CryptoUtil.Remove(memoryStream.ToArray(), 0))!;
        IEnumerable<string> authorList = fabricMod?.authors.Select(delegate (JsonElement element) {
            if (element.ValueKind == JsonValueKind.String) {
                return element.GetString();
            }
            return (!element.TryGetProperty("name", out element)) ? null : element.GetString();
        })!;
        string authors = ((fabricMod?.authors != null) ? string.Join(", ", authorList) : null)!;
        return new ModPack {
            Id = fabricMod?.name!,
            Description = ((fabricMod != null) ? fabricMod.description.Split('.')[0] : null)!,
            Version = fabricMod?.version!,
            Url = fabricMod?.contact?.homepage!,
            Authors = authors
        };
    }

    private static ModPack LoadQuiltMod(Stream infoStream) {
        using StreamReader reader = new(infoStream);
        string json = reader.ReadToEnd();

        JsonNode node = JsonNode.Parse(json)!["quilt_loader"]!["metadata"]!;
        return new ModPack {
            Id = node["name"]!.GetValue<string>(),
            Description = node["description"]!.GetValue<string>(),
            Version = node["version"]!.GetValue<string>(),
            Url = node["contact"]!["homepage"]!.GetValue<string>()! ?? node["contact"]!["issues"]?.GetValue<string>()!,
            Authors = "6"
        };
    }

    private static ModPack LoadForgeModByInfo(Stream infoStream) {
        using MemoryStream memoryStream = new MemoryStream();
        infoStream.CopyTo(memoryStream);
        ForgeMod forgeMod = JsonSerializer.Deserialize<ForgeMod[]>(CryptoUtil.Remove(memoryStream.ToArray(), 1))![0];
        if (forgeMod?.modList != null) {
            forgeMod = forgeMod.modList[0];
        }
        string[] authorList = forgeMod?.authorList ?? forgeMod?.authors!;
        string authors = ((authorList != null) ? string.Join(", ", authorList) : null)!;
        return new ModPack {
            Id = forgeMod?.name!,
            Description = ((forgeMod != null) ? forgeMod.description.Split('.')[0] : null)!,
            Version = forgeMod?.version!,
            GameVersion = forgeMod?.mcversion!,
            Url = forgeMod?.url!,
            Authors = authors
        };
    }

    private static ModPack LoadForgeModByToml(Stream infoStream) {
        using StreamReader reader = new(infoStream);
        var tomlContent = reader.ReadToEnd();
        TomlFileParser parser = new(tomlContent);
        if (parser.Select("mods")) {
            return new ModPack {
                Id = parser["displayName"],
                DisplayName = parser["displayName"],
                Description = parser["description"],
                Url = parser["displayURL"],
                Version = parser["version"],
                Authors = string.Join(",", parser["authors"]?.Split(",").Select(x => x.Trim(' '))!)
            };
        }

        return default;
    }


    public ModPackUtil(GameCore? Id, bool Isolate) {
        GameCore = Id;
        IsCopy = true;
        IsOlate = Isolate;
        ModPacksDirectory = Id!.GetModsPath(Isolate);
        WorkingDirectory = Id.GetGameCorePath();
    }

    public ModPackUtil(GameCore? Id) {
        GameCore = Id;
        IsCopy = true;
        IsOlate = true;
        ModPacksDirectory = Id!.GetModsPath(false);
        WorkingDirectory = Id.GetGameCorePath();
    }
}
