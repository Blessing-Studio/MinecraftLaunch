using Flurl;
using Flurl.Http;
using MinecraftLaunch.Extensions;
using MinecraftLaunch.Utilities;
using System.Text.Json.Nodes;

namespace MinecraftLaunch.Components.Provider;

public sealed class CurseforgeProvider {
    public static string CurseforgeApiKey = string.Empty;
    internal static string CurseforgeApi = "https://api.curseforge.com/v1";

    internal static async Task<JsonNode> GetModFileEntryAsync(long modId, long fileId, CancellationToken cancellationToken = default) {
        string requestUrl = $"https://api.curseforge.com/v1/mods/{modId}/files/{fileId}";
        string json = string.Empty;

        try {
            using var responseMessage = await HttpUtil.FlurlClient.Request(requestUrl)
                .WithHeader("x-api-key", CurseforgeApiKey)
                .GetAsync(cancellationToken: cancellationToken);

            json = await responseMessage.GetStringAsync();
        } catch (Exception) { }

        return json?.AsNode()?.Select("data");
    }

    internal static async Task<string> GetModDownloadUrlAsync(long modId, long fileId, CancellationToken cancellationToken = default) {
        string requestUrl = $"https://api.curseforge.com/v1/mods/{modId}/files/{fileId}/download-url";
        string json = string.Empty;

        try {
            using var responseMessage = await HttpUtil.FlurlClient.Request(requestUrl)
                .WithHeader("x-api-key", CurseforgeApiKey)
                .GetAsync(cancellationToken: cancellationToken);

            json = await responseMessage.GetStringAsync();
        } catch (FlurlHttpException ex) {
            if (ex.StatusCode is 403)
                return string.Empty;
        }

        return json?.AsNode()?.GetString("data")
            ?? throw new InvalidModpackFileException();
    }

    internal static async Task<string> TestDownloadUrlAsync(long fileId, string fileName, CancellationToken cancellationToken = default) {
        var fileIdStr = fileId.ToString();
        List<string> urls = [
            $"https://edge.forgecdn.net/files/{fileIdStr[..4]}/{fileIdStr[4..]}/{fileName}",
            $"https://mediafiles.forgecdn.net/files/{fileIdStr[..4]}/{fileIdStr[4..]}/{fileName}"
        ];

        try {
            foreach (var url in urls) {
                var response = await HttpUtil.FlurlClient.Request(url)
                    .HeadAsync(cancellationToken: cancellationToken);

                if (!response.ResponseMessage.IsSuccessStatusCode)
                    continue;

                return url;
            }
        } catch (Exception) {}

        throw new InvalidOperationException();
    }
}

[Serializable]
public class InvalidModpackFileException : Exception {
    public long ProjectId { get; set; }

    public InvalidModpackFileException() { }
    public InvalidModpackFileException(string message) : base(message) { }
    public InvalidModpackFileException(string message, Exception inner) : base(message, inner) { }
}