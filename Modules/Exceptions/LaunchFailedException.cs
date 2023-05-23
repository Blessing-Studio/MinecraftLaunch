using System;
using System.Runtime.Serialization;

namespace MinecraftLaunch.Modules.Exceptions;

[Serializable]
public class LaunchFailedException : Exception
{
	public LaunchFailedException()
	{
	}

	public LaunchFailedException(string message)
		: base(message)
	{
	}

	public LaunchFailedException(string message, Exception inner)
		: base(message, inner)
	{
	}

	protected LaunchFailedException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
