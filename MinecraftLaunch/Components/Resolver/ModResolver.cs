using System.IO.Compression;
using System.Text.Json.Nodes;
using MinecraftLaunch.Extensions;
using System.Collections.Immutable;
using MinecraftLaunch.Classes.Interfaces;
using MinecraftLaunch.Classes.Models.Game;

namespace MinecraftLaunch.Components.Resolver {
    public class ModResolver(GameEntry entry) : IResolver<ModEntry> {
        private readonly GameEntry _gameEntry = entry;

        private readonly TomlResolver _tomlResolver = new();

        public ModEntry Resolve(string str) {
            using var zip = ZipFile.OpenRead(str);
            var result = new ModEntry {
                Path = str,
                IsEnabled = Path.GetExtension(str).Equals(".jar")
            };

            try {
                if (zip.Entries.Any(e => e.Name == "mods.toml")) {
                    OfForgeModEntry(zip, ref result);
                } else if (zip.Entries.Any(e => e.Name == "mcmod.info")) {
                    OfLegacyForgeModEntry(zip.GetEntry("mcmod.info").ReadAsString(), ref result);
                } else if (zip.Entries.Any(e => e.Name is "fabric.mod.json")) {
                    OfFabricModEntry(zip, ref result);
                } else if (zip.Entries.Any(e => e.Name == "quilt.mod.json")) {
                    OfQulitModEntry(zip, ref result);
                }

                return result;
            }
            catch (Exception) {
                return null;
            }
        }

        public Task<ImmutableArray<ModEntry>> LoadAllAsync() {
            List<ModEntry> entries = new();
            var mods = Directory.EnumerateFiles(_gameEntry
                .OfModDirectorypath());

            Parallel.ForEach(mods, path => {
                entries.Add(Resolve(path));
            });

            return Task.FromResult(entries.Where(x => x is not null).ToImmutableArray());
        }

        public static bool Switch(ModEntry entry, bool isEnable) {
            var rawFilePath = entry.Path;

            entry.IsEnabled = isEnable;
            entry.Path = Path.Combine(Path.GetDirectoryName(rawFilePath), Path
                .GetFileNameWithoutExtension(rawFilePath) + (isEnable ? ".jar" : ".disabled"));

            try {
                File.Move(rawFilePath, entry.Path);
                return true;
            }
            catch (Exception) {
                return false;
            }
        }

        public static void Delete(ModEntry entry) {
            File.Delete(entry.Path);
        }

        private void OfLegacyForgeModEntry(string text, ref ModEntry entry) {
            var jsonNode = text.Replace("\u000a", "").AsNode()
                .AsArray()
                .FirstOrDefault()
                ?? throw new InvalidDataException("Invalid mcmod.info");

            try {
                entry.DisplayName = jsonNode.GetString("name");
                entry.Version = jsonNode.GetString("version");
                entry.Description = jsonNode.GetString("description");
                entry.Authors = (jsonNode["authorList"] ?? jsonNode["authors"])
                    .GetEnumerable<string>()
                    .ToImmutableArray();
            }
            catch (Exception) {
                entry.IsError = true;
            }
        }

        private void OfFabricModEntry(ZipArchive zipArchive, ref ModEntry entry) {
            var zipEntry = zipArchive.GetEntry("fabric.mod.json");
            using var stream = zipEntry.Open();
            using (var reader = new StreamReader(stream)) {
                try {
                    var jsonNode = reader.ReadToEnd().AsNode();

                    entry.DisplayName = jsonNode.GetString("name");
                    entry.Version = jsonNode.GetString("version");
                    entry.Description = jsonNode.GetString("description");
                    entry.Authors = jsonNode["authors"]
                        .GetEnumerable<string>()
                        .ToImmutableArray();
                }
                catch (Exception) {
                    entry.IsError = true;
                }
            }
        }

        private void OfForgeModEntry(ZipArchive zipArchive, ref ModEntry entry) {
            //var zipEntry = zipArchive.GetEntry("META-INF/mods.toml");
            //using var stream = zipEntry.Open();
            //using (var reader = new StreamReader(stream)) {
            //    try {
            //        string tomlText = reader.ReadToEnd();
            //        _tomlResolver.Content = tomlText;
            //
            //        if (_tomlResolver.Select("mods")) {
            //            entry.Description = _tomlResolver["description"];
            //            entry.DisplayName = _tomlResolver["displayName"];
            //            entry.Authors = _tomlResolver["authors"]
            //                .Split(",")
            //                .Select(x => x.Trim(' '))
            //                .ToImmutableArray();
            //        }
            //    }
            //    catch (Exception) {
            //        entry.IsError = true;
            //    }
            //}
        }

        private void OfQulitModEntry(ZipArchive zipArchive, ref ModEntry entry) {
            var zipEntry = zipArchive.GetEntry("quilt.mod.json");
            using var stream = zipEntry.Open();
            using (var reader = new StreamReader(stream)) {
                try {
                    var jsonNode = reader.ReadToEnd().AsNode();
                    jsonNode = jsonNode["quilt_loader"]["metadata"];

                    entry.DisplayName = jsonNode?.GetString("name");
                    entry.Version = jsonNode?.GetString("version");
                    entry.Description = jsonNode?.GetString("description");
                    entry.Authors = jsonNode["authors"]
                        .GetEnumerable<string>()
                        .ToImmutableArray();
                }
                catch (Exception) {
                    entry.IsError = true;
                }
            }
        }
    }
}
