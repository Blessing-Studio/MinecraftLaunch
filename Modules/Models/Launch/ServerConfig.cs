namespace MinecraftLaunch.Modules.Models.Launch;

public class ServerConfig
{
	public int Port { get; set; }

	public string Ip { get; set; }

	public ServerConfig(int port, string ip)
	{
		Port = port;
		Ip = ip;
	}

	public ServerConfig(string ip)
	{
		Ip = ip;
	}

	public ServerConfig()
	{
	}
}
