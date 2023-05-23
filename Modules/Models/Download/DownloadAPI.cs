namespace MinecraftLaunch.Modules.Models.Download;

public class DownloadAPI
{
	public string Host { get; set; }

	public string VersionManifest { get; set; }

	public string Assets { get; set; }

	public string Libraries { get; set; }

    public override bool Equals(object? obj) {
		var result = obj as DownloadAPI;
		return (Host == result.Host && VersionManifest == result.VersionManifest && Assets == result.Assets && Libraries == result.Libraries);
    }
}
