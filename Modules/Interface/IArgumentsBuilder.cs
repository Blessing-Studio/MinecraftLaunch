using System.Collections.Generic;

namespace MinecraftLaunch.Modules.Interface;

public interface IArgumentsBuilder
{
	/// <summary>
	/// �����������
	/// </summary>
	/// <returns></returns>
	IEnumerable<string?> Build();
	/// <summary>
	/// ����ǰ�ò���
	/// </summary>
	/// <returns></returns>
	IEnumerable<string?> GetFrontArguments();
	/// <summary>
	/// �������ò���
	/// </summary>
	/// <returns></returns>
	IEnumerable<string?> GetBehindArguments();
}
