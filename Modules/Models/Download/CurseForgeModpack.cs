using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace MinecraftLaunch.Modules.Models.Download;

public class CurseForgeModpack
{
	[JsonProperty("id")]
	public int Id { get; set; }

	[JsonProperty("name")]
	public string Name { get; set; }

	[JsonProperty("summary")]
	public string Description { get; set; }

	[JsonProperty("links")]
	public Dictionary<string, string> Links { get; set; }

	[JsonProperty("downloadCount")]
	public int DownloadCount { get; set; }

	[JsonProperty("dateModified")]
	public DateTime LastUpdateTime { get; set; }

	[JsonProperty("gamePopularityRank")]
	public int GamePopularityRank { get; set; }

	[JsonProperty("latestFilesIndexes")]
	public List<CurseForgeModpackFileInfo> LatestFilesIndexes { get; set; }

	[JsonProperty("categories")]
	public List<CurseForgeModpackCategory> Categories { get; set; }

	public string IconUrl { get; set; }

	public Dictionary<string, List<CurseForgeModpackFileInfo>> Files { get; set; } = new Dictionary<string, List<CurseForgeModpackFileInfo>>();


	public string[] SupportedVersions { get; set; }

	public override string ToString()
	{
		try
		{
			TimeSpan timeSpan = DateTime.Now - LastUpdateTime;
			List<int> types = new List<int>();
			Files.Values.ToList().ForEach(delegate(List<CurseForgeModpackFileInfo> x)
			{
				x.ForEach(delegate(CurseForgeModpackFileInfo y)
				{
					if (y.ModLoaderType.HasValue && !types.Contains((int)y.ModLoaderType.Value))
					{
						types.Add((int)y.ModLoaderType.Value);
					}
				});
			});
			IEnumerable<string> modLoaderTypes = from x in types
				select x switch
				{
					0 => "All", 
					1 => "Forge", 
					2 => "Cauldron", 
					3 => "LiteLoader", 
					4 => "Fabric", 
					_ => string.Empty, 
				} into x
				where !string.IsNullOrEmpty(x)
				select x;
			StringBuilder stringBuilder = new StringBuilder().Append(modLoaderTypes.Any() ? ("[" + string.Join(',', modLoaderTypes) + "]") : string.Empty);
			StringBuilder timeBuilder = new StringBuilder().Append((timeSpan.Days != 0) ? $"{timeSpan.Days} 天" : string.Empty).Append((timeSpan.Hours != 0) ? $" {timeSpan.Hours} 小时" : string.Empty);
			string downloadCount = ((DownloadCount > 1000) ? $"{DownloadCount / 1000}k" : DownloadCount.ToString());
			StringBuilder stringBuilder2 = stringBuilder.Append((stringBuilder.Length > 0) ? " " : string.Empty).Append(SupportedVersions.Any() ? ("[" + SupportedVersions.First() + ((SupportedVersions.First() == SupportedVersions.Last()) ? string.Empty : ("-" + SupportedVersions.Last())) + "]") : string.Empty).Append(" ");
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(5, 1, stringBuilder2);
			handler.AppendFormatted(timeBuilder.ToString());
			handler.AppendLiteral(" 前更新，");
			StringBuilder stringBuilder3 = stringBuilder2.Append(ref handler);
			StringBuilder.AppendInterpolatedStringHandler handler2 = new StringBuilder.AppendInterpolatedStringHandler(4, 1, stringBuilder3);
			handler2.AppendFormatted(downloadCount);
			handler2.AppendLiteral(" 次下载");
			return stringBuilder3.Append(ref handler2).ToString();
		}
		catch
		{
			return null;
		}
	}
}

public class CurseForgeResourcePack : CurseForgeModpack
{

}
