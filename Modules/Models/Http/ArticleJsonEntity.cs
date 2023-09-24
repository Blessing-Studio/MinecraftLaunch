using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftLaunch.Modules.Models.Http {
    public class ArticleJsonEntity {
        [JsonPropertyName("default_tile")]
        public DefaultTileJsonEntity DefaultTile { get; set; }

        [JsonPropertyName("articleLang")]
        public string Lang { get; set; }

        [JsonPropertyName("primary_category")]
        public string PrimaryCategory { get; set; }

        [JsonPropertyName("categories")]
        public List<string> Categories { get; set; }

        [JsonPropertyName("article_url")]
        public string NewsUrl { get; set; }

        [JsonPropertyName("publish_date")]
        public string PublishDate { get; set; }

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; }

        public string ImageUrl { get; set; }
    }
}
