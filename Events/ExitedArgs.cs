using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;

namespace MinecraftLaunch.Events;

public class ExitedArgs
{
	public int ExitCode { get; set; }

	public bool Crashed { get; set; }

	[JsonIgnore]
	public Stopwatch RunTime { get; set; }

	public IEnumerable<string> Outputs { get; set; }
}
