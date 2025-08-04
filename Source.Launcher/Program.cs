using Game.Client;
using Game.Server;

using Microsoft.Extensions.DependencyInjection;

using Source.Common;
using Source.Engine;

namespace Source.Launcher;

public class Bootloader {
	ICommandLine commandLine;
	IEngineAPI? engineAPI;

	string baseDir;
	bool isEditMode;
	bool isTextMode;
	
	public Bootloader() {
		commandLine = new CommandLine();
		commandLine.CreateCmdLine(Environment.CommandLine);
		GetBaseDirectory(commandLine, out baseDir);
		isTextMode = commandLine.CheckParm("-textmode");
	}
	public void Boot() {
		

		bool needsRestart;
		do {

			engineAPI = new EngineBuilder(commandLine, baseDir, false)
				.WithGameDLL<ServerGameDLL>()
				.WithClientDLL<HLClient>()
				.Build(false);
			BuildStartupInfo();
			var res = engineAPI.Run();
			needsRestart = res == IEngineAPI.Result.InitRestart || res == IEngineAPI.Result.RunRestart;
		} while (needsRestart);
	}

	static void GetBaseDirectory(ICommandLine cmdLine, out string baseDirectory) {
		baseDirectory = cmdLine.CheckParm("-basedir", out var values) ? values.FirstOrDefault() ?? AppContext.BaseDirectory : AppContext.BaseDirectory;
	}

	private void BuildStartupInfo() {
		StartupInfo info = new();
		info.BaseDirectory = baseDir;
		info.InitialMod = DetermineInitialMod();
		info.InitialGame = DetermineInitialGame();
		info.TextMode = isTextMode;

		engineAPI!.SetStartupInfo(in info);
	}

	const string defaultHalfLife2GameDirectory = "hl2";

	private string DetermineInitialMod() {
		return !isEditMode ? commandLine.ParmValue("-game", defaultHalfLife2GameDirectory) : throw new NotImplementedException("No editmode support");
	}

	private string DetermineInitialGame() {
		return !isEditMode ? commandLine.ParmValue("-game", defaultHalfLife2GameDirectory) : throw new NotImplementedException("No editmode support");
	}
}

internal class Program
{
	static void Main(string[] _) {
		Bootloader bootloader = new();
		bootloader.Boot();
	}
}