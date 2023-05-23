namespace MinecraftLaunch.Modules.Models.Download;

public class ForgeMod
{
	public ForgeMod[] modList { get; set; }

	public string name { get; set; }

	public string description { get; set; }

	public string version { get; set; }

	public string mcversion { get; set; }

	public string[] authors { get; set; }

	public string[] authorList { get; set; }

	public string url { get; set; }
}
