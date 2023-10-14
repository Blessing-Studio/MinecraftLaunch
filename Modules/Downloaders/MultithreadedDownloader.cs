using MinecraftLaunch.Modules.Models.Download;
using MinecraftLaunch.Modules.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace MinecraftLaunch.Modules.Downloaders {
    public class MultithreadedDownloader<T> {
        private readonly Dictionary<DownloadRequest, FileDownloaderResponse> FailedDownloadRequests = new();

        public int MaxThreadNumber { get; set; } = 128;

        public int BufferCapacity { get; set; } = 256;

        public List<T> Sources { get; private set; }

        public Func<T, DownloadRequest> HandleFunc { get; private set; }

        public event EventHandler Completed;

        public event EventHandler<FileDownloaderResponse> SingleDownloaded;

        public event EventHandler<(float, string)> ProgressChanged;

        public MultithreadedDownloader(Func<T, DownloadRequest> func, List<T> sources) {
            HandleFunc = func;
            Sources = sources;
        }

        public async Task<MultithreadedDownloadResponse> DownloadAsync() {
            TransformManyBlock<List<T>, DownloadRequest> transformManyBlock = new TransformManyBlock<List<T>, DownloadRequest>((List<T> x) => x.Select((T x) => HandleFunc(x)));
            int post = 0;
            int output = 0;
            ActionBlock<DownloadRequest> actionBlock = new ActionBlock<DownloadRequest>(async delegate (DownloadRequest request) {
                post++;
                if (!request.Directory.Exists) {
                    request.Directory.Create();
                }

                FileDownloaderResponse httpDownloadResponse = null;
                try {
                    httpDownloadResponse = await FileDownloader.DownloadAsync(request);
                    if (httpDownloadResponse.HttpStatusCode != HttpStatusCode.OK) {
                        FailedDownloadRequests.Add(request, httpDownloadResponse);
                    }

                    this.SingleDownloaded?.Invoke(this, httpDownloadResponse);
                }
                catch {
                    if (httpDownloadResponse.HttpStatusCode != HttpStatusCode.OK) {
                        FailedDownloadRequests.Add(request, httpDownloadResponse);
                    }
                }

                output++;
                this.ProgressChanged?.Invoke(this, ((float)output / (float)post, $"{output}/{post}"));
            }, new ExecutionDataflowBlockOptions {
                BoundedCapacity = BufferCapacity,
                MaxDegreeOfParallelism = MaxThreadNumber
            });
            IDisposable disposable = transformManyBlock.LinkTo(actionBlock, new DataflowLinkOptions {
                PropagateCompletion = true
            });
            transformManyBlock.Post(Sources);
            transformManyBlock.Complete();
            await actionBlock.Completion;
            this.Completed?.Invoke(this, null);
            disposable.Dispose();
            GC.Collect();
            return new MultithreadedDownloadResponse {
                IsAllSuccess = !FailedDownloadRequests.Any(),
                FailedDownloadRequests = FailedDownloadRequests
            };
        }
    }
}
