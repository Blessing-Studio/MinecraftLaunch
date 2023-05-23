using System;

namespace MinecraftLaunch.Modules.Interface;

public class ProgressChangedEventArgs : EventArgs
{
	public string? ProgressDescription { get; set; }

	public float Progress { get; set; }
}
