using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace MinecraftLaunch.Modules.Models.Http {
    public class DefaultTileJsonEntity {
        [JsonPropertyName("image")]
        public object image { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("tile_size")]
        public string TileSize { get; set; }

        [JsonPropertyName("sub_header")]
        public string SubHeader { get; set; }
    }
}
