using Source.Common.Server;
using Source.Engine;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Server;

public static class GameInterface
{
	public static object? GetCommandClient(this Util Util) {
		int idx = Util.GetCommandClientIndex();
		if (idx > 0)
			return Util.PlayerByIndex(idx);

		return null;
	}

	public static int GetCommandClientIndex(this Util Util) {
		return 0;
	}

	public static object? PlayerByIndex(this Util Util, int idx) {
		return null;
	}
}

public class ServerGameDLL : IServerGameDLL
{
	public void PostInit() {

	}
}
