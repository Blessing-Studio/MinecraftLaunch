using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftLaunch.Modules.Models.Auth
{
    public class OwnershipItem
    {
        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("signature")] public string Signature { get; set; }
    }

    public class GameHasCheckResponseModel
    {
        [JsonProperty("items")] public List<OwnershipItem> Items { get; set; }

        [JsonProperty("signature")] public string Signature { get; set; }

        [JsonProperty("keyId")] public string KeyId { get; set; }
    }
}
