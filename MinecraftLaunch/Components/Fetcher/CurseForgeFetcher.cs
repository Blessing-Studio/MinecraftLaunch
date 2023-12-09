using System.Text.Json;
using System.Text.Json.Nodes;
using MinecraftLaunch.Extensions;
using MinecraftLaunch.Classes.Interfaces;
using MinecraftLaunch.Classes.Models.Download;

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

            entry.IconUrl = node["logo"]?.GetString("url");
            entry.WebLink = node["links"]?.GetString("websiteUrl");
            entry.Authors = node?.GetEnumerable<string>("authors", "name");
            entry.Categories = node?.GetEnumerable<string>("categories", "name");
            entry.ScreenshotUrls = node?.GetEnumerable<string>("screenshots", "url");
            entry.Files = entry.Files.Select(x => {
                x.ModId = entry.Id;
                return x;
            });

            return entry;
        }
    }
}
