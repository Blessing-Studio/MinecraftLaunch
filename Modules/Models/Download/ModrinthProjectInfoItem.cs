using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MinecraftLaunch.Modules.Models.Download;

public class ModrinthProjectInfoItem
{
	[JsonProperty("id")]
	public string Id { get; set; }

	[JsonProperty("project_id")]
	public string ProjectId { get; set; }

	[JsonProperty("author_id")]
	public string AuthorId { get; set; }

	[JsonProperty("featured")]
	public bool Featured { get; set; }

	[JsonProperty("name")]
	public string Name { get; set; }

	[JsonProperty("version_number")]
	public string VersionNumber { get; set; }

	[JsonProperty("changelog")]
	public string ChangeLog { get; set; }

	[JsonProperty("changelog_url")]
	public string ChangeLogUrl { get; set; }

	[JsonProperty("date_published")]
	public DateTime PublishDate { get; set; }

	[JsonProperty("downloads")]
	public int Downloads { get; set; }

	[JsonProperty("version_type")]
	public string VersionType { get; set; }

	[JsonProperty("files")]
	public List<ModrinthFileInfo> Files { get; set; }

	[JsonProperty("loaders")]
	public List<string> Loaders { get; set; }

	[JsonProperty("dependencies")]
	public List<ModrinthDependency> Dependencies { get; set; }

	[JsonProperty("game_versions")]
	public List<string> GameVersion { get; set; }
}
