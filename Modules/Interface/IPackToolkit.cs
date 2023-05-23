using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace MinecraftLaunch.Modules.Interface;

public interface IPackToolkit<T>
{
	ValueTask<ImmutableArray<T>> LoadAllAsync();

	ValueTask<ImmutableArray<T>> MoveLoadAllAsync(IEnumerable<string> paths);
}
