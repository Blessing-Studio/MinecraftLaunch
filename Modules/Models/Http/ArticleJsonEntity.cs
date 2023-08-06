using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftLaunch.Modules.Models.Http {
    public class ArticleJsonEntity {
        [JsonProperty("default_tile")]
        public DefaultTileJsonEntity default_tile { get; set; }

        [JsonProperty("articleLang")]
        public string Lang { get; set; }

        [JsonProperty("primary_category")]
        public string PrimaryCategory { get; set; }

        [JsonProperty("categories")]
        public List<string> Categories { get; set; }

        [JsonProperty("article_url")]
        public string NewsUrl { get; set; }

        [JsonProperty("publish_date")]
        public string PublishDate { get; set; }

        [JsonProperty("tags")]
        public List<string> Tags { get; set; }

        public string ImageUrl { get; set; }
    }
}
