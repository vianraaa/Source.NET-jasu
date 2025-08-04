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
				.WithClient<HLClient>
				.WithServer<ServerGameDLL>
				.Build(false);

			var res = engine.Run();
			needsRestart = res == IEngineAPI.Result.InitRestart || res == IEngineAPI.Result.RunRestart;
		} while (needsRestart);
    }
}