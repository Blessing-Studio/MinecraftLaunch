using System.Text;
using MinecraftLaunch.Modules.Enum;
using MinecraftLaunch.Modules.Models.Download;
using Natsurainko.Toolkits.Network;
using Newtonsoft.Json.Linq;

namespace MinecraftLaunch.Modules.Toolkits
{
    public partial class CurseForgeToolkit
    {
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
        public async ValueTask<List<CurseForgeModpack>> GetFeaturedsAsync()
        {
            var result = new List<CurseForgeModpack>();

            var content = new JObject {           
                { "gameId", 432 },
                { "excludedModIds", JToken.FromObject(new int[] { 0 }) },
                { "gameVersionTypeId", null }
            };

            try {           
                using var responseMessage = await HttpWrapper.HttpPostAsync($"{API}/featured", content.ToString(), Headers);
                responseMessage.EnsureSuccessStatusCode();

                var entity = JObject.Parse(await responseMessage.Content.ReadAsStringAsync());
                
                foreach (JObject jObject in ((JArray)entity["data"]["popular"]).Cast<JObject>())
                    result.Add(ParseCurseForgeModpack(jObject));

                foreach (JObject jObject in ((JArray)entity["data"]["recentlyUpdated"]).Cast<JObject>())
                    result.Add(ParseCurseForgeModpack(jObject));

                foreach (JObject jObject in ((JArray)entity["data"]["featured"]).Cast<JObject>())
                    result.Add(ParseCurseForgeModpack(jObject));

                result.Sort((a, b) => a.GamePopularityRank.CompareTo(b.GamePopularityRank));

                return result;
            }
            catch { }

            return null;
        }

        /// <summary>
        /// 资源基础搜索方法
        /// </summary>
        /// <remarks>
        /// 所有的搜索方法都会直接使用此方法
        /// </remarks>
        /// <returns></returns>
        public async ValueTask<List<CurseForgeModpack>> SearchResourceAsync(string searchFilter, int classId, ModLoaderType modLoaderType = ModLoaderType.Any, string gameVersion = null, int category = -1) {
            var builder = new StringBuilder(API)
                          .Append($"/search?gameId=432")
                          .Append(string.IsNullOrEmpty(searchFilter) ? string.Empty : $"&searchFilter={searchFilter}")
                          .Append((int)modLoaderType == 8 ? $"&modLoaderType={(int)modLoaderType}" : string.Empty)
                          .Append(string.IsNullOrEmpty(gameVersion) ? string.Empty : $"&gameVersion={gameVersion}")
                          .Append(category == -1 ? string.Empty : $"&categoryId={gameVersion}")
                          .Append($"&sortField=Featured&sortOrder=desc&classId={classId}");

            var result = new List<CurseForgeModpack>();

            try
            {
                using var responseMessage = await HttpWrapper.HttpGetAsync(builder.ToString(), Headers);
                responseMessage.EnsureSuccessStatusCode();

                var entity = JObject.Parse(await responseMessage.Content.ReadAsStringAsync());
                ((JArray)entity["data"]).ToList().ForEach(x => result.Add(ParseCurseForgeModpack((JObject)x)));

                result.Sort((a, b) => a.GamePopularityRank.CompareTo(b.GamePopularityRank));

                return result;
            }
            catch { }

            return null;

        }

        /// <summary>
        /// 获取模组下载链接
        /// </summary>
        /// <param name="addonId"></param>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public async ValueTask<string> GetModpackDownloadUrlAsync(long addonId, long fileId)
        {
            string reqUrl = $"{API}/{addonId}/files/{fileId}/download-url";
            using HttpResponseMessage res = await HttpWrapper.HttpGetAsync(reqUrl, Headers);
            return (await res.Content.ReadAsStringAsync()).ToJsonEntity<DataModel<string>>()?.Data!;
        }

        public async ValueTask<List<CurseForgeModpackCategory>> GetCategories()
        {
            try
            {
                using var responseMessage = await HttpWrapper.HttpGetAsync($"https://api.curseforge.com/v1/categories?gameId=432", Headers);
                responseMessage.EnsureSuccessStatusCode();

                var entity = JObject.Parse(await responseMessage.Content.ReadAsStringAsync());

                return ((JArray)entity["data"]).Select(x => x.ToObject<CurseForgeModpackCategory>()).ToList();
            }
            catch { }

            return null;
        }

        public async ValueTask<string> GetModDescriptionHtmlAsync(int modId)
        {
            string url = $"{API}/{modId}/description";
            try
            {
                using HttpResponseMessage responseMessage = await HttpWrapper.HttpGetAsync(url, Headers);
                responseMessage.EnsureSuccessStatusCode();
                return (await responseMessage.Content.ReadAsStringAsync()).ToJsonEntity<DataModel<string>>().Data;
            }
            catch
            {
            }
            return null;
        }

        protected CurseForgeModpack ParseCurseForgeModpack(JObject entity)
        {
            var modpack = entity.ToObject<CurseForgeModpack>();

            if (entity.ContainsKey("logo") && entity["logo"].Type != JTokenType.Null)
                modpack.IconUrl = (string)entity["logo"]["url"];

            modpack.LatestFilesIndexes.ForEach(x =>
            {
                x.DownloadUrl = $"https://edge.forgecdn.net/files/{x.FileId.ToString().Insert(4, "/")}/{x.FileName}";

                if (!modpack.Files.ContainsKey(x.SupportedVersion))
                    modpack.Files.Add(x.SupportedVersion, new());

                modpack.Files[x.SupportedVersion].Add(x);
            });

            modpack.Links.Where(x => string.IsNullOrEmpty(x.Value)).Select(x => x.Key).ToList().ForEach(x => modpack.Links.Remove(x));
            modpack.Files = modpack.Files.OrderByDescending(x => (int)(float.Parse(x.Key.Substring(2)) * 100)).ToDictionary(x => x.Key, x => x.Value);
            modpack.SupportedVersions = modpack.Files.Keys.ToArray();

            return modpack;
        }
    }

    partial class CurseForgeToolkit
    {
        private const string API = "https://api.curseforge.com/v1/mods";

        public static string Key { get; set; } = string.Empty;

        private Dictionary<string, string> Headers => new Dictionary<string, string> { { "x-api-key", Key } };

        public CurseForgeToolkit(string accesskey) {       
            Key = accesskey;
        }
    }
}