using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftLaunch.Modules.Models.Http {
    public class McVersionUpdateJsonEntity {
        [JsonPropertyName("article_count")]
        public int ArticleCount { get; set; }

        [JsonPropertyName("article_grid")]
        public List<ArticleJsonEntity> Articles { get; set; }
    }
}
