using System.Collections.Generic;

namespace MinecraftLaunch.Modules.Interface;

public interface IArgumentsBuilder
{
	/// <summary>
	/// 构建整体参数
	/// </summary>
	/// <returns></returns>
	IEnumerable<string?> Build();
	/// <summary>
	/// 构建前置参数
	/// </summary>
	/// <returns></returns>
	IEnumerable<string?> GetFrontArguments();
	/// <summary>
	/// 构建后置参数
	/// </summary>
	/// <returns></returns>
	IEnumerable<string?> GetBehindArguments();
}
