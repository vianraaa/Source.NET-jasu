using Game.Client;
using Game.Server;

using Source.FileSystem;

using Source.Common;
using Source.Common.Commands;
using Source.Common.Engine;
using Source.Common.Filesystem;
using Source.Engine;

using Steamworks;
using Source.Common.Launcher;
using Source.SDLManager;
using Source.Common.Input;
using Nucleus.SDL3Window;
using Source.Common.ShaderAPI;
using Source.ShaderAPI.GL46;

namespace Source.Launcher;

public class Bootloader : IDisposable
{
	ICommandLine commandLine;
	IEngineAPI? engineAPI;

	string baseDir;
	bool isEditMode;
	bool isTextMode;

	public Bootloader() {
		commandLine = new CommandLine();
		commandLine.CreateCmdLine(Environment.CommandLine);
		GetBaseDirectory(commandLine, out baseDir);
		SteamAPI.Init();
		isTextMode = commandLine.CheckParm("-textmode");
	}
	public void Boot() {
		bool needsRestart;
		do {
			engineAPI = new EngineBuilder(commandLine)
				.WithComponent<IFileSystem, BaseFileSystem>()
				.WithComponent<ILauncherManager, SDL3_LauncherManager>()
				.WithComponent<IInputSystem, SDL3_InputSystem>()
				.WithComponent<IShaderAPI, ShaderAPI_GL46>()
				.WithGameDLL<ServerGameDLL>()
				.WithClientDLL<HLClient>()
				.Build(dedicated: false);
			PreInit();
			var res = engineAPI.Run();
			needsRestart = res == IEngineAPI.Result.InitRestart || res == IEngineAPI.Result.RunRestart;
		} while (needsRestart);
	}

	static void GetBaseDirectory(ICommandLine cmdLine, out string baseDirectory) {
		baseDirectory = cmdLine.CheckParm("-basedir", out var values) ? values.FirstOrDefault() ?? AppContext.BaseDirectory : AppContext.BaseDirectory;
	}

	private void PreInit() {
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
	public void Dispose() {
		SteamAPI.Shutdown();
	}
}

internal class Program
{
	static void Main(string[] _) {
		using (Bootloader bootloader = new())
			bootloader.Boot();
	}
}