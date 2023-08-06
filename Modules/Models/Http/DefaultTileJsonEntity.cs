using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace MinecraftLaunch.Modules.Models.Http {
    public class DefaultTileJsonEntity {
        [JsonProperty("image")]
        public object image { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("tile_size")]
        public string TileSize { get; set; }

        [JsonProperty("sub_header")]
        public string SubHeader { get; set; }
    }
}
