using Game.Client;
using Game.Server;

using Source.Common;
using Source.Engine;

namespace Source.Launcher;

internal class Program
{
    static void Main(string[] _)
    {
		bool needsRestart;
		do {
			EngineAPI engine = new EngineBuilder()
				.WithGameDLL<ServerGameDLL>()
				.WithClientDLL<HLClient>()
				.Build(false);

			var res = engine.Run();
			needsRestart = res == IEngineAPI.Result.InitRestart || res == IEngineAPI.Result.RunRestart;
		} while (needsRestart);
    }
}