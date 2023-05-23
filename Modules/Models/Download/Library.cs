namespace MinecraftLaunch.Modules.Models.Download;

public class Library
{
	public string Name { get; set; }

	public LibraryType Type { get; set; }

	public string Path { get; set; }

	public int Size { get; set; }

	public string SHA1 { get; set; }

	public string Url { get; set; }

	public string[] Exclude { get; set; }

	public override bool Equals(object obj)
	{
		return (obj as Library)?.Name == Name;
	}

	public override int GetHashCode()
	{
		return Name.GetHashCode();
	}
}
