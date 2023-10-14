using System.Text;
using System.Text.Json;
using Flurl;
using Flurl.Http;
using MinecraftLaunch.Modules.Enum;
using MinecraftLaunch.Modules.Models.Download;

namespace MinecraftLaunch.Modules.Utilities {
    public partial class CurseForgeUtil {
        private const string API = "https://api.curseforge.com/v1/mods";

        public static string Key { get; set; } = string.Empty;

        public CurseForgeUtil(string accesskey) {
            Key = accesskey;
        }

        /// <summary>
        /// 模组搜索方法
        /// </summary>
        /// <param name="searchFilter"></param>
        /// <param name="modLoaderType"></param>
        /// <param name="gameVersion"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        public async ValueTask<List<CurseForgeModpack>> SearchModpackAsync(string searchFilter, ModLoaderType modLoaderType = ModLoaderType.Any, string gameVersion = null, int category = -1) {
            return await SearchResourceAsync(searchFilter, 6, modLoaderType, gameVersion, category);
        }

        /// <summary>
        /// 整合包搜索方法
        /// </summary>
        /// <param name="searchFilter"></param>
        /// <param name="modLoaderType"></param>
        /// <param name="gameVersion"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        public async ValueTask<List<CurseForgeModpack>> SearchModpacksAsync(string searchFilter, ModLoaderType modLoaderType = ModLoaderType.Any, string gameVersion = null, int category = -1) {
            return await SearchResourceAsync(searchFilter, 4471, modLoaderType, gameVersion, category);
        }

        /// <summary>
        /// 资源包搜索方法
        /// </summary>
        /// <param name="searchFilter"></param>
        /// <param name="modLoaderType"></param>
        /// <param name="gameVersion"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        public async ValueTask<List<CurseForgeModpack>> SearchResourcePackAsync(string searchFilter, string gameVersion = null, int category = -1) {
            return await SearchResourceAsync(searchFilter, 12, ModLoaderType.Unknown, gameVersion, category);
        }

        /// <summary>
        /// 游戏地图搜索方法
        /// </summary>
        /// <param name="searchFilter"></param>
        /// <param name="modLoaderType"></param>
        /// <param name="gameVersion"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        public async ValueTask<List<CurseForgeModpack>> SearchGameMapAsync(string searchFilter, string gameVersion = null, int category = -1) {
            return await SearchResourceAsync(searchFilter, 17, ModLoaderType.Unknown, gameVersion, category);
        }

        /// <summary>
        /// 获取热门资源方法
        /// </summary>
        /// <returns></returns>
        public async ValueTask<List<CurseForgeModpack>> GetFeaturedsAsync() {
            var result = new List<CurseForgeModpack>();

            var content = new {
                gameId = 432,
                excludedModIds = new int[] { 0 },
                gameVersionTypeId = null as string
            };

            try {
                using var responseMessage = await $"{API}/featured"
                    .WithHeader("x-api-key", Key)
                    .PostJsonAsync(content);

                responseMessage.ResponseMessage
                    .EnsureSuccessStatusCode();

                var entitys = JsonDocument.Parse(await responseMessage.GetStringAsync()).RootElement;
                var data = entitys.GetProperty("data");

                result.AddRange(data.GetProperty("popular")
                    .EnumerateArray()
                    .Select(ParseCurseForgeModpack));

                result.AddRange(data.GetProperty("recentlyUpdated")
                    .EnumerateArray()
                    .Select(ParseCurseForgeModpack));

                result.AddRange(data.GetProperty("featured")
                    .EnumerateArray()
                    .Select(ParseCurseForgeModpack));

                result.Sort((a, b) => a.GamePopularityRank.CompareTo(b.GamePopularityRank));

                return result;
            }
            catch { }

            return default!;
        }

        /// <summary>
        /// 资源基础搜索方法
        /// </summary>
        /// <remarks>
        /// 所有的搜索方法都会直接使用此方法
        /// </remarks>
        /// <returns></returns>
        public async ValueTask<List<CurseForgeModpack>> SearchResourceAsync(string searchFilter, int classId, ModLoaderType modLoaderType = ModLoaderType.Any, string gameVersion = null, int category = -1) {
            var parameters = new List<string> {
                "gameId=432",
                string.IsNullOrEmpty(searchFilter) ? string.Empty : $"searchFilter={searchFilter}",
                (int)modLoaderType == 8 ? $"modLoaderType={(int)modLoaderType}" : string.Empty,
                string.IsNullOrEmpty(gameVersion) ? string.Empty : $"gameVersion={gameVersion}",
                category == -1 ? string.Empty : $"categoryId={gameVersion}",
                "sortField=Featured",
                "sortOrder=desc",
                $"classId={classId}"
            };

            var url = $"{API}/search?" + string.Join("&", parameters.Where(p => p != null));
            var result = new List<CurseForgeModpack>();

            try {
                using var responseMessage = await url
                    .WithHeader("x-api-key", Key)
                    .GetAsync();

                responseMessage.ResponseMessage.EnsureSuccessStatusCode();

                var entity = JsonDocument.Parse(await responseMessage
                    .GetStringAsync()).RootElement;

                foreach (var x in entity.GetProperty("data").EnumerateArray())
                    result.Add(ParseCurseForgeModpack(x));

                result.Sort((a, b) => a.GamePopularityRank.CompareTo(b.GamePopularityRank));

                return result;
            }
            catch { }

            return null!;
        }

        /// <summary>
        /// 获取模组下载链接
        /// </summary>
        /// <param name="addonId"></param>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public async ValueTask<string> GetModpackDownloadUrlAsync(long addonId, long fileId) {
            string reqUrl = $"{API}/{addonId}/files/{fileId}/download-url";
            using var responseMessage = await reqUrl.ToString()
                .WithHeader("x-api-key", Key)
                .GetAsync();

            return (await responseMessage.GetStringAsync()).ToJsonEntity<DataModel<string>>()?.Data!;
        }

        public async ValueTask<List<CurseForgeModpackCategory>> GetCategories() {
            try {
                using var responseMessage = await $"https://api.curseforge.com/v1/categories?gameId=432"
                    .WithHeader("x-api-key", Key)
                    .GetAsync();

                responseMessage.ResponseMessage.EnsureSuccessStatusCode();
                var entity = JsonDocument.Parse(await responseMessage.GetStringAsync()).RootElement;

                return entity.GetProperty("data").EnumerateArray().Select(x => JsonSerializer.Deserialize<CurseForgeModpackCategory>(x
                    .GetRawText()))
                    .ToList()!;
            }
            catch { }

            return null;
        }

        public async ValueTask<string> GetModDescriptionHtmlAsync(int modId) {
            string url = $"{API}/{modId}/description";
            try {
                using var responseMessage = await url.WithHeader("x-api-key", Key)
                    .GetAsync();

                responseMessage.ResponseMessage.EnsureSuccessStatusCode();
                return (await responseMessage.GetStringAsync()).ToJsonEntity<DataModel<string>>().Data;
            }
            catch {
            }

            return default!;
        }

        protected CurseForgeModpack ParseCurseForgeModpack(JsonElement entity) {
            var modpack = JsonSerializer.Deserialize<CurseForgeModpack>(entity.GetRawText());

            if (entity.TryGetProperty("logo", out var logo) && logo.ValueKind != JsonValueKind.Null)
                modpack!.IconUrl = logo.GetProperty("url").GetString()!;

            modpack!.LatestFilesIndexes.ForEach(x => {
                x.DownloadUrl = $"https://edge.forgecdn.net/files/{x.FileId.ToString().Insert(4, "/")}/{x.FileName}";

                if (!modpack.Files.ContainsKey(x.SupportedVersion))
                    modpack.Files.Add(x.SupportedVersion, new());

                modpack.Files[x.SupportedVersion].Add(x);
            });

            modpack.Links = modpack.Links.Where(x => !string.IsNullOrEmpty(x.Value))
                .ToDictionary(x => x.Key, x => x.Value);

            modpack.Files = modpack.Files.OrderByDescending(x => (int)(float.Parse(x.Key
                .Substring(2)) * 100))
                .ToDictionary(x => x.Key, x => x.Value);

            modpack.SupportedVersions = modpack.Files
                .Keys.ToArray();

            return modpack;
        }
    }
}