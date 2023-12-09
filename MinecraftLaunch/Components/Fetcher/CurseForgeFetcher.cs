using MinecraftLaunch.Classes.Interfaces;
using MinecraftLaunch.Classes.Models.Download;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace MinecraftLaunch.Components.Fetcher {
    public class CurseForgeFetcher : IFetcher<IEnumerable<CurseForgeResourceEntry>> {
        public IEnumerable<CurseForgeResourceEntry> Fetch() {
            return FetchAsync().GetAwaiter().GetResult();
        }

        public ValueTask<IEnumerable<CurseForgeResourceEntry>> FetchAsync() {
            throw new NotImplementedException();
        }

        private CurseForgeResourceEntry ResolveFromJsonNode(JsonNode node) {
            var entry = node.Deserialize<CurseForgeResourceEntry>();

            entry.WebLink = node["links"]?["websiteUrl"]?.GetValue<string>();
            entry.IconUrl = node["logo"]?["url"]?.GetValue<string>();
            entry.Authors = node["authors"]?.AsArray().Select(x => x["name"].GetValue<string>());
            entry.ScreenshotUrls = node["screenshots"]?.AsArray().Select(x => x["url"].GetValue<string>());
            entry.Categories = node["categories"]?.AsArray().Select(x => x["name"].GetValue<string>());
            entry.Files = entry.Files.Select(x => {
                x.ModId = entry.Id;
                return x;
            });

            return entry;
        }
    }
}
