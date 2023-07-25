using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MinecraftLaunch.Modules.Interface;
using MinecraftLaunch.Modules.Models.Download;
using MinecraftLaunch.Modules.Models.Launch;
using MinecraftLaunch.Modules.Utils;

namespace MinecraftLaunch.Modules.ArgumentsBuilders;

public sealed partial class JavaMinecraftArgumentsBuilder : IArgumentsBuilder
{
	public static readonly IEnumerable<string> DefaultAdvancedArguments = new string[8] { "-XX:-OmitStackTraceInFastThrow", "-XX:-DontCompileHugeMethods", "-Dfile.encoding=GB18030", "-Dfml.ignoreInvalidMinecraftCertificates=true", "-Dfml.ignorePatchDiscrepancies=true", "-Djava.rmi.server.useCodebaseOnly=true", "-Dcom.sun.jndi.rmi.object.trustURLCodebase=false", "-Dcom.sun.jndi.cosnaming.object.trustURLCodebase=false" };

	public static readonly IEnumerable<string> DefaultGCArguments = new string[7] { "-XX:+UseG1GC", "-XX:+UnlockExperimentalVMOptions", "-XX:G1NewSizePercent=20", "-XX:G1ReservePercent=20", "-XX:MaxGCPauseMillis=50", "-XX:G1HeapRegionSize=16m", "-XX:-UseAdaptiveSizePolicy" };

    public JavaMinecraftArgumentsBuilder(GameCore? gameCore, LaunchConfig? launchConfig)
    {
        GameCore = gameCore;
        LaunchConfig = launchConfig;
    }

    public GameCore? GameCore { get; private set; }

	public LaunchConfig? LaunchConfig { get; private set; }

	public IEnumerable<string> Build()
	{
		foreach (string frontArgument in GetFrontArguments())
            yield return frontArgument;

        yield return GameCore.MainClass;

		foreach (string behindArgument in GetBehindArguments())
            yield return behindArgument;
    }

    public IEnumerable<string> GetBehindArguments()
	{
        var keyValuePairs = new Dictionary<string, string>()
        {
            { "${auth_player_name}" , this.LaunchConfig.Account.Name },
            { "${version_name}" , this.GameCore.Id },
            { "${assets_root}" , Path.Combine(this.GameCore.Root.FullName, "assets").ToPath() },
            { "${assets_index_name}" , Path.GetFileNameWithoutExtension(this.GameCore.AssetIndexFile.FileInfo.FullName) },
            { "${auth_uuid}" , this.LaunchConfig.Account.Uuid.ToString("N") },
            { "${auth_access_token}" , this.LaunchConfig.Account.AccessToken },
            { "${user_type}" , "Mojang" },
            { "${version_type}" , this.GameCore.Type },
            { "${user_properties}" , "{}" },
            { "${game_assets}" , Path.Combine(this.GameCore.Root.FullName, "assets").ToPath() },
            { "${auth_session}" , this.LaunchConfig.Account.AccessToken },
			{ "${game_directory}" , LaunchConfig.IsEnableIndependencyCore ? Path.Combine(GameCore.Root.FullName,"versions",GameCore.Id) : GameCore.Root.FullName },
        };

        List<string> list = GameCore.BehindArguments.ToList();
		
		if (LaunchConfig.GameWindowConfig != null)
		{
			list.Add($"--width {LaunchConfig.GameWindowConfig.Width}");
			list.Add($"--height {LaunchConfig.GameWindowConfig.Height}");
			if (LaunchConfig.GameWindowConfig.IsFullscreen)
			{
				list.Add("--fullscreen");
			}
		}
		
		if (LaunchConfig.ServerConfig != null && !string.IsNullOrEmpty(LaunchConfig.ServerConfig.Ip) && LaunchConfig.ServerConfig.Port != 0)
		{
			list.Add("--server " + LaunchConfig.ServerConfig.Ip);
			list.Add("--port " + LaunchConfig.ServerConfig.Port);
		}
		
		foreach (string item in list)
            yield return item.Replace(keyValuePairs);
    }

    public IEnumerable<string?> GetFrontArguments()
	{
        var keyValuePairs = new Dictionary<string, string>()
        {
            { "${launcher_name}", "MinecraftLaunch" },
            { "${launcher_version}", "3" },
            { "${classpath_separator}", Path.PathSeparator.ToString() },
            { "${classpath}", this.GetClasspath().ToPath() },
            { "${client}", this.GameCore.ClientFile.FileInfo.FullName.ToPath() },
            { "${min_memory}", this.LaunchConfig.JvmConfig.MinMemory.ToString() },
            { "${max_memory}", this.LaunchConfig.JvmConfig.MaxMemory.ToString() },
            { "${library_directory}", Path.Combine(this.GameCore.Root.FullName, "libraries").ToPath() },
            {
                "${version_name}",
                string.IsNullOrEmpty(this.GameCore.InheritsFrom)
                ? this.GameCore.Id
                : this.GameCore.InheritsFrom
            },
            {
                "${natives_directory}",
                this.LaunchConfig.NativesFolder != null && this.LaunchConfig.NativesFolder.Exists
                ? this.LaunchConfig.NativesFolder.FullName.ToString()
                : Path.Combine(this.GameCore.Root.FullName, "versions", this.GameCore.Id, "natives").ToPath()
            }
        };

        if (!Directory.Exists(keyValuePairs["${natives_directory}"]))
            Directory.CreateDirectory(keyValuePairs["${natives_directory}"].Trim('"'));

        List<string> args = new string[3] { "-Xmn${min_memory}m", "-Xmx${max_memory}m", "-Dminecraft.client.jar=${client}" }.ToList();

		foreach (string item4 in GetEnvironmentJvmArguments())
            args.Add(item4);

        if (LaunchConfig.JvmConfig.GCArguments == null)
            DefaultGCArguments.ToList().ForEach(x => args.Add(x));
        else
            LaunchConfig.JvmConfig.GCArguments.ToList().ForEach(x => args.Add(x));

        if (LaunchConfig.JvmConfig.AdvancedArguments == null)
            DefaultAdvancedArguments.ToList().ForEach(x => args.Add(x));
        else
            LaunchConfig.JvmConfig.AdvancedArguments.ToList().ForEach(x => args.Add(x));

        args.Add("-Dlog4j2.formatMsgNoLookups=true");
		foreach (string item3 in GameCore.FrontArguments)
            args.Add(item3);

        foreach (string item2 in args)
            yield return item2.Replace(keyValuePairs);
    }

	internal string GetClasspath()
	{
		List<IResource> loads = new List<IResource>();
		GameCore.LibraryResources.ForEach(delegate(LibraryResource x)
		{
			if (x.IsEnable && !x.IsNatives)
			{
				loads.Add(x);
			}
		});
		loads.Add(GameCore.ClientFile);
		return string.Join(Path.PathSeparator.ToString(), loads.Select((IResource x) => x.ToFileInfo().FullName));
	}

	internal static IEnumerable<string> GetEnvironmentJvmArguments()
	{
		string platformName = EnvironmentUtil.GetPlatformName();
		if (!(platformName == "windows"))
		{
			if (platformName == "osx")
                yield return "-XstartOnFirstThread";
        }
        else
		{
			yield return "-XX:HeapDumpPath=MojangTricksIntelDriversForPerformance_javaw.exe_minecraft.exe.heapdump";
			if (Environment.OSVersion.Version.Major == 10)
			{
				yield return "-Dos.name=\"Windows 10\"";
				yield return "-Dos.version=10.0";
			}
		}
		if (EnvironmentUtil.Arch == "32")
            yield return "-Xss1M";
    }
}
