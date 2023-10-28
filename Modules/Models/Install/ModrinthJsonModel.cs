using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MinecraftLaunch.Modules.Models.Install {
    public record ModrinthJsonModel {
        [JsonPropertyName("formatVersion")]
        public int FormatVersion { get; set; }

        [JsonPropertyName("game")]
        public string Game { get; set; }

        [JsonPropertyName("versionId")]
        public string VersionId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("files")]
        public IEnumerable<Files> Files { get; set; }

        [JsonPropertyName("dependencies")]
        public Dependencies Dependencies { get; set; }
    }

    public record Dependencies {
        [JsonPropertyName("minecraft")]
        public string Minecraft { get; set; }

        [JsonPropertyName("quilt-loader")]
        public string QuiltLoader { get; set; } = string.Empty;

        [JsonPropertyName("fabric-loader")]
        public string FabricLoader { get; set; } = string.Empty;
    }

    public record Files {
        [JsonPropertyName("fileSize")]
        public int FileSize { get; set; }

        [JsonPropertyName("path")]
        public string Path { get; set; }

        [JsonPropertyName("hashes")]
        public Hashes Hashes { get; set; }

        [JsonPropertyName("env")]
        public Env Env { get; set; }

        [JsonPropertyName("downloads")]
        public string[] Downloads { get; set; }

    }

    public record Hashes {
        [JsonPropertyName("sha1")]
        public string Sha1 { get; set; }

        [JsonPropertyName("sha512")]
        public string Sha512 { get; set; }
    }

    public record Env {
        [JsonPropertyName("client")]
        public string Client { get; set; }

        [JsonPropertyName("server")]
        public string Server { get; set; }
    }
}
