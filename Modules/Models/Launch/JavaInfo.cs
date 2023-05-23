namespace MinecraftLaunch.Modules.Models.Launch;

public class JavaInfo
{
	public string JavaVersion { get; set; }

	public string JavaDirectoryPath { get; set; }

	public string JavaPath { get; set; }

	public int JavaSlugVersion { get; set; }

	public bool Is64Bit { get; set; }

	public override string ToString()
	{
		return $"Java路径：{JavaPath} Java版本全名：{JavaVersion} Java版本简写：{JavaSlugVersion} 是否为64位：{Is64Bit}";
	}
}
