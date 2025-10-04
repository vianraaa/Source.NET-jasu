using Source;
using Source.Common.Commands;
using Source.Engine;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Server;

[EngineComponent]
public class GameServerClientMethods(Host Host)
{
	[ConCommand(helpText: "Display player message")]
	void say(in TokenizedCommand args, CommandSource source, int clientslot) {
		if (clientslot == 0)
			Host.Say(0, in args, false);
	}
}

public static class HostExts {
	public static void Say(this Host host, object? edict, in TokenizedCommand args, bool teamOnly) {

	}
}

public static class ServerClient {
	[ConCommand(helpText: "Noclip. Player becomes non-solid and flies.")]
	static void noclip() {

	}
}