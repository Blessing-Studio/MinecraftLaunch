using System;
using MinecraftLaunch.Modules.Interface;

namespace MinecraftLaunch.Events;

public class BaseProcessOutput : IProcessOutput
{
	public string Raw { get; private set; }

	public BaseProcessOutput(string output)
	{
		Raw = output;
	}

	public string GetPrintValue()
	{
		return Raw;
	}

	public void Print()
	{
		Console.WriteLine(Raw);
	}
}
