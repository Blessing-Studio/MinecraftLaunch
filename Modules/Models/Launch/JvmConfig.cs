using System.Collections.Generic;
using System.IO;

namespace MinecraftLaunch.Modules.Models.Launch;

public class JvmConfig
{
	public FileInfo JavaPath { get; set; }

	public int MaxMemory { get; set; } = 1024;


	public bool UsedGC { get; set; } = true;


	public int MinMemory { get; set; } = 512;


	public IEnumerable<string> AdvancedArguments { get; set; }

	public IEnumerable<string> GCArguments { get; set; }

	public JvmConfig()
	{
	}

	public JvmConfig(string file)
	{
		JavaPath = new FileInfo(file);
	}

	public JvmConfig(FileInfo fileInfo)
	{
		JavaPath = fileInfo;
	}
}
