using Game.Client;
using Game.Server;

using Source.Common;
using Source.Engine;

namespace Source.Launcher;

internal class Program
{
	static void GetBaseDirectory(ICommandLine cmdLine, out string baseDirectory) {
		baseDirectory = cmdLine.CheckParm("-basedir", out string? value) ? value : AppContext.BaseDirectory;
	}

	static void Main(string[] _)
    {
		ICommandLine commandLine = new CommandLine();
		commandLine.CreateCmdLine(Environment.CommandLine);
		string baseDirectory;
		GetBaseDirectory(commandLine, out baseDirectory);

		bool isTextMode = commandLine.CheckParm("-textmode") != null;

		bool needsRestart;
		do {
			
			EngineAPI engine = new EngineBuilder(commandLine, AppContext.BaseDirectory, false)
				.WithGameDLL<ServerGameDLL>()
				.WithClientDLL<HLClient>()
				.Build(false);

			var res = engine.Run();
			needsRestart = res == IEngineAPI.Result.InitRestart || res == IEngineAPI.Result.RunRestart;
		} while (needsRestart);
    }
}