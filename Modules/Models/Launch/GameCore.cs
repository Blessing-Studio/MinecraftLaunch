using System.Collections.Generic;
using System.IO;
using MinecraftLaunch.Modules.Models.Download;

namespace MinecraftLaunch.Modules.Models.Launch;

public class GameCore
{
	public DirectoryInfo? Root { get; set; }

	public FileResource? ClientFile { get; set; }

	public FileResource? AssetIndexFile { get; set; }

	public FileResource? LogConfigFile { get; set; }

	public List<LibraryResource>? LibraryResources { get; set; }

	public string? MainClass { get; set; }

	public IEnumerable<ModLoaderInfo> ModLoaderInfos { get; set; } = new ModLoaderInfo[0];

	public IEnumerable<string> FrontArguments { get; set; } = new string[0];

	public IEnumerable<string> BehindArguments { get; set; } = new string[0];

	public string? Id { get; set; }

	public string? Type { get; set; }

	public int? JavaVersion { get; set; }

	public string? InheritsFrom { get; set; }

	public string? Source { get; set; }

	public bool HasModLoader { get; set; }

    public override bool Equals(object? obj) {   
        return (obj as GameCore)!.Id!.Equals(Id);
    }
}
