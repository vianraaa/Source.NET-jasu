using Source.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Game.Shared;

public static class HandleExts {
	static IClientEntityList? entityList;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IHandleEntity? Get(this BaseHandle handle) {
		return (entityList ??= Singleton<IClientEntityList>()).LookupEntity(handle);
	}
}

public class Handle : BaseHandle {

}