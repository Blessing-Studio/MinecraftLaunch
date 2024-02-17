using MinecraftLaunch.Classes.Models.Download;
using MinecraftLaunch.Classes.Models.Event;

namespace MinecraftLaunch.Classes.Interfaces
{
    public interface IDownloader
    {
        event Action<bool> Completed;
        event EventHandler<DownloadProgressChangedEventArgs> ProgressChanged;

        void Cancel();
        ValueTask<bool> DownloadAsync();
        void Retry();
        void Setup(IEnumerable<DownloadRequest> downloadItems);
    }
}