using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Natsurainko.Toolkits.Network;
using Natsurainko.Toolkits.Network.Model;
using Natsurainko.Toolkits.Values;

namespace MinecraftLaunch.Modules.Utils;

public class HttpUtil {
    private static readonly HttpClient HttpClient = new HttpClient();

    public static int BufferSize { get; set; } = 1048576;

    public static async ValueTask<string> GetStringAsync(string Uri) {
        return await HttpClient.GetStringAsync(Uri);
    }

    public static async ValueTask<HttpResponseMessage> HttpGetAsync(string url, Tuple<string, string> authorization = null, HttpCompletionOption httpCompletionOption = HttpCompletionOption.ResponseContentRead) {
        using HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
        if (authorization != null) {
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue(authorization.Item1, authorization.Item2);
        }
        HttpResponseMessage httpResponseMessage = await HttpClient.SendAsync(requestMessage, httpCompletionOption, CancellationToken.None);
        if (httpResponseMessage.StatusCode.Equals(HttpStatusCode.Found)) {
            string absoluteUri = httpResponseMessage.Headers.Location.AbsoluteUri;
            httpResponseMessage.Dispose();
            GC.Collect();
            return await HttpGetAsync(absoluteUri, authorization, httpCompletionOption);
        }
        return httpResponseMessage;
    }

    public static async ValueTask<HttpResponseMessage> HttpGetAsync(string url, Dictionary<string, string> headers, HttpCompletionOption httpCompletionOption = HttpCompletionOption.ResponseContentRead) {
        using HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
        if (headers != null && headers.Any()) {
            foreach (KeyValuePair<string, string> header in headers) {
                requestMessage.Headers.Add(header.Key, header.Value);
            }
        }
        HttpResponseMessage httpResponseMessage = await HttpClient.SendAsync(requestMessage, httpCompletionOption, CancellationToken.None);
        if (httpResponseMessage.StatusCode.Equals(HttpStatusCode.Found)) {
            string absoluteUri = httpResponseMessage.Headers.Location.AbsoluteUri;
            httpResponseMessage.Dispose();
            GC.Collect();
            return await HttpGetAsync(absoluteUri, headers, httpCompletionOption);
        }
        return httpResponseMessage;
    }

    public static async ValueTask<HttpResponseMessage> HttpPostAsync(string url, Stream content, string contentType = "application/json") {
        HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, url);
        StreamContent httpContent = new StreamContent(content);
        httpContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        httpRequestMessage.Content = httpContent;
        HttpResponseMessage result = await HttpClient.SendAsync(httpRequestMessage);
        content.Dispose();
        httpContent.Dispose();
        return result;
    }

    public static async ValueTask<HttpResponseMessage> HttpPostAsync(string url, string content, string contentType = "application/json") {
        HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, url);
        using StringContent httpContent = new StringContent(content);
        httpContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        httpRequestMessage.Content = httpContent;
        return await HttpClient.SendAsync(httpRequestMessage);
    }

    public static async ValueTask<HttpResponseMessage> HttpPostAsync(string url, string content, Dictionary<string, string> headers, string contentType = "application/json") {
        HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, url);
        using StringContent httpContent = new StringContent(content);
        httpContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        if (headers != null && headers.Any()) {
            foreach (KeyValuePair<string, string> header in headers) {
                httpRequestMessage.Headers.Add(header.Key, header.Value);
            }
        }
        httpRequestMessage.Content = httpContent;
        return await HttpClient.SendAsync(httpRequestMessage);
    }

    public static async ValueTask<HttpDownloadResponse> HttpDownloadAsync(string url, string folder, string filename = null) {
        FileInfo fileInfo = null;
        HttpResponseMessage responseMessage = null;
        try {
            responseMessage = await HttpWrapper.HttpGetAsync(url, new Dictionary<string, string>(), HttpCompletionOption.ResponseHeadersRead);
            responseMessage.EnsureSuccessStatusCode();
            fileInfo = ((responseMessage.Content.Headers == null || responseMessage.Content.Headers.ContentDisposition == null) ? new FileInfo(Path.Combine(folder, Path.GetFileName(responseMessage.RequestMessage.RequestUri.AbsoluteUri))) : new FileInfo(Path.Combine(folder, responseMessage.Content.Headers.ContentDisposition.FileName.Trim(new char[1] { '"' }))));
            if (filename != null) {
                fileInfo = new FileInfo(fileInfo.FullName.Replace(fileInfo.Name, filename));
            }
            if (!Directory.Exists(folder)) {
                Directory.CreateDirectory(folder);
            }
            using FileStream fileStream = File.Create(fileInfo.FullName);
            using Stream stream = await responseMessage.Content.ReadAsStreamAsync();
            byte[] bytes = new byte[BufferSize];
            for (int num = await stream.ReadAsync(bytes, 0, BufferSize); num > 0; num = await stream.ReadAsync(bytes, 0, BufferSize)) {
                await fileStream.WriteAsync(bytes, 0, num);
            }
            fileStream.Flush();
            responseMessage.Dispose();
            return new HttpDownloadResponse {
                FileInfo = fileInfo,
                HttpStatusCode = responseMessage.StatusCode,
                Message = responseMessage.ReasonPhrase + "[" + url + "]"
            };
        }
        catch (HttpRequestException ex) {
            return new HttpDownloadResponse {
                FileInfo = fileInfo,
                HttpStatusCode = (responseMessage?.StatusCode).Value,
                Message = ex.Message + "[" + url + "]"
            };
        }
        catch (Exception ex2) {
            return new HttpDownloadResponse {
                FileInfo = fileInfo,
                HttpStatusCode = HttpStatusCode.GatewayTimeout,
                Message = ex2.Message + "[" + url + "]"
            };
        }
    }

    public static async ValueTask<HttpDownloadResponse> HttpDownloadAsync(HttpDownloadRequest request) {
        return await HttpDownloadAsync(request.Url, request.Directory.FullName, request.FileName);
    }

    public static async ValueTask<HttpDownloadResponse> HttpDownloadAsync(string url, string folder, Action<float, string> progressChangedAction, string filename = null) {
        Action<float, string> progressChangedAction2 = progressChangedAction;
        FileInfo fileInfo = null;
        HttpResponseMessage responseMessage = null;
        using System.Timers.Timer timer = new System.Timers.Timer(1000.0);
        _ = 4;
        try {
            responseMessage = await HttpWrapper.HttpGetAsync(url, new Dictionary<string, string>(), HttpCompletionOption.ResponseHeadersRead);
            responseMessage.EnsureSuccessStatusCode();
            fileInfo = ((responseMessage.Content.Headers == null || responseMessage.Content.Headers.ContentDisposition == null) ? new FileInfo(Path.Combine(folder, Path.GetFileName(responseMessage.RequestMessage.RequestUri.AbsoluteUri))) : new FileInfo(Path.Combine(folder, responseMessage.Content.Headers.ContentDisposition.FileName.Trim(new char[1] { '"' }))));
            if (filename != null) {
                fileInfo = new FileInfo(fileInfo.FullName.Replace(fileInfo.Name, filename));
            }
            if (!Directory.Exists(folder)) {
                Directory.CreateDirectory(folder);
            }
            FileStream fileStream = File.Create(fileInfo.FullName);
            try {
                using Stream stream = await responseMessage.Content.ReadAsStreamAsync();
                timer.Elapsed += delegate {
                    progressChangedAction2((float)fileStream.Length / (float)responseMessage.Content.Headers.ContentLength.Value, LongExtension.LengthToMb(fileStream.Length) + " / " + LongExtension.LengthToMb(responseMessage.Content.Headers.ContentLength.Value));
                };
                timer.Start();
                byte[] bytes = new byte[BufferSize];
                for (int num = await stream.ReadAsync(bytes, 0, BufferSize); num > 0; num = await stream.ReadAsync(bytes, 0, BufferSize)) {
                    await fileStream.WriteAsync(bytes, 0, num);
                }
                fileStream.Flush();
                responseMessage.Dispose();
                timer.Stop();
                return new HttpDownloadResponse {
                    FileInfo = fileInfo,
                    HttpStatusCode = responseMessage.StatusCode,
                    Message = responseMessage.ReasonPhrase + "[" + url + "]"
                };
            }
            finally {
                if (fileStream != null) {
                    ((IDisposable)fileStream).Dispose();
                }
            }
        }
        catch (HttpRequestException ex) {
            if (timer.Enabled) {
                timer.Stop();
            }
            return new HttpDownloadResponse {
                FileInfo = fileInfo,
                HttpStatusCode = (responseMessage?.StatusCode).Value,
                Message = ex.Message + "[" + url + "]"
            };
        }
        catch (Exception ex2) {
            if (timer.Enabled) {
                timer.Stop();
            }
            return new HttpDownloadResponse {
                FileInfo = fileInfo,
                HttpStatusCode = HttpStatusCode.GatewayTimeout,
                Message = ex2.Message + "[" + url + "]"
            };
        }
    }

    public static async ValueTask<HttpDownloadResponse> HttpDownloadAsync(HttpDownloadRequest request, Action<float, string> progressChangedAction) {
        return await HttpDownloadAsync(request.Url, request.Directory.FullName, progressChangedAction, request.FileName);
    }

    public static async ValueTask<HttpResponseMessage> HttpSimulateBrowserGetAsync(string url) {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.AcceptEncoding.Add(new("gzip"));
        request.Headers.Connection.Add("keep-alive");

        return await HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
    }
}
