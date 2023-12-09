using Flurl.Http;

namespace MinecraftLaunch.Extensions {
    public static class HttpExtension {
        private static readonly HttpClient _httpClient = new();

        public static async Task<IFlurlResponse> PostUrlEncodedAsync(this string url,
            HttpContent content = default,
            CancellationToken? source = default) {
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url) {
                Content = content
            };

            var result = await _httpClient.SendAsync(request, source ?? new());
            return new FlurlResponse(new() {
                HttpResponseMessage = result
            });
        }
    }
}
