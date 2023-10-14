using MinecraftLaunch.Modules.Models.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace MinecraftLaunch.Modules.Utilities {
    public class McNewsUtil {
        private readonly static string ImageBaseUrl = "https://www.minecraft.net";

        private readonly static string McVersionUpdateAPI = "https://www.minecraft.net/content/minecraft-net/_jcr_content.articles.grid?tileselection=auto&pageSize=50&tagsPath=minecraft:stockholm/minecraft";

        public static async ValueTask<McVersionUpdateJsonEntity> GetMcVersionUpdatesAsync() {
            using var httpResponse = await HttpUtil.HttpSimulateBrowserGetAsync(McVersionUpdateAPI);
            using var stream = await httpResponse.Content.ReadAsStreamAsync();
            var json = StringUtil.ConvertGzipStreamToString(stream);
            var mcVersionUpdateEntity = json.ToJsonEntity<McVersionUpdateJsonEntity>();

            // Concurrency get news's image from minecraft website
            var actionBlock = new ActionBlock<ArticleJsonEntity>(async articleInfo => {
                using var httpResponse = await HttpUtil.HttpSimulateBrowserGetAsync($"{ImageBaseUrl}{articleInfo.NewsUrl}");
                using var stream = await httpResponse.Content.ReadAsStreamAsync();

                var htmlStrs = StringUtil.ConvertGzipStreamToList(stream);
                foreach (var htmlStr in htmlStrs.AsParallel()) {
                    if (htmlStr.Contains("og:image")) {
                        articleInfo.ImageUrl = StringUtil.GetPropertyFromHtmlText(htmlStr, "meta", "content");
                    }
                }
            }, new ExecutionDataflowBlockOptions {
                BoundedCapacity = 64,
                MaxDegreeOfParallelism = 64
            });

            foreach (var article in mcVersionUpdateEntity.Articles.AsParallel()) {
                actionBlock.Post(article);
            }

            actionBlock.Complete();

            await actionBlock.Completion;
            return mcVersionUpdateEntity;
        }
    }
}
