namespace MinecraftLaunch.Modules.Interface;

public interface IProcessOutput
{
	string Raw { get; }

	string GetPrintValue();

	void Print();
}
