namespace MinecraftLaunch.Classes.Interfaces;

public interface IDownloader {
    ValueTask<bool> DownloadAsync();
}