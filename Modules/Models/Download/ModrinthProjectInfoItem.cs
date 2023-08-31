using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MinecraftLaunch.Modules.Models.Download;

public class ModrinthProjectInfoItem
{
	[JsonPropertyName("id")]
	public string Id { get; set; }

	[JsonPropertyName("project_id")]
	public string ProjectId { get; set; }

	[JsonPropertyName("author_id")]
	public string AuthorId { get; set; }

	[JsonPropertyName("featured")]
	public bool Featured { get; set; }

	[JsonPropertyName("name")]
	public string Name { get; set; }

	[JsonPropertyName("version_number")]
	public string VersionNumber { get; set; }

	[JsonPropertyName("changelog")]
	public string ChangeLog { get; set; }

	[JsonPropertyName("changelog_url")]
	public string ChangeLogUrl { get; set; }

	[JsonPropertyName("date_published")]
	public DateTime PublishDate { get; set; }

	[JsonPropertyName("downloads")]
	public int Downloads { get; set; }

	[JsonPropertyName("version_type")]
	public string VersionType { get; set; }

	[JsonPropertyName("files")]
	public List<ModrinthFileInfo> Files { get; set; }

	[JsonPropertyName("loaders")]
	public List<string> Loaders { get; set; }

	[JsonPropertyName("dependencies")]
	public List<ModrinthDependency> Dependencies { get; set; }

	[JsonPropertyName("game_versions")]
	public List<string> GameVersion { get; set; }
}
