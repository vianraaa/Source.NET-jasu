using Microsoft.Extensions.DependencyInjection;

using Source.Common;
using Source.Common.Client;
using Source.Common.Engine;
using Source.Common.Filesystem;
using Source.Engine.Client;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source.Engine;

/// <summary>
/// Screen library.
/// </summary>
public class Scr(IEngineAPI engineAPI)
{
	public bool Initialized;
	public bool DisabledForLoading;
	public bool DrawLoading;
	public int NextDrawTick;

	Host Host;
	Sys Sys;
	IEngineVGuiInternal? EngineVGui;
	Con Con;
	IHostState HostState;
	IFileSystem FileSystem;
	IBaseClientDLL clientDll;
	ClientState cl;
	ClientGlobalVariables clientGlobalVariables;

	public void Init() {
		Initialized = true;

		Host = engineAPI.GetRequiredService<Host>();
		Sys = engineAPI.GetRequiredService<Sys>();
		EngineVGui = engineAPI.GetRequiredService<IEngineVGuiInternal>();
		Con = engineAPI.GetRequiredService<Con>();
		HostState = engineAPI.GetRequiredService<IHostState>();
		FileSystem = engineAPI.GetRequiredService<IFileSystem>();
		clientDll = engineAPI.GetRequiredService<IBaseClientDLL>();
		cl = engineAPI.GetRequiredService<ClientState>();
		clientGlobalVariables = engineAPI.GetRequiredService<ClientGlobalVariables>();
	}

	public void BeginLoadingPlaque() {
		if (!DrawLoading) {
			// EngineVGui.SetNotAllowedToShowGameUI(false);
			Host.AllowQueuedMaterialSystem(false);
			DrawLoading = true;

			// todo: sounds

			Con.ClearNotify();
			CenterStringOff();

			if (clientDll != null)
				clientDll.HudText(null);

			EngineVGui?.OnLevelLoadingStarted();

			clientGlobalVariables.FrameTime = 0;

			clientGlobalVariables.FrameCount = ++Host.FrameCount;

			// Ensures the screen is painted to reflect the loading state
			UpdateScreen();
			clientGlobalVariables.FrameCount = ++Host.FrameCount;
			UpdateScreen();

			clientGlobalVariables.FrameTime = cl.GetFrameTime();

			DisabledForLoading = true;
		}
	}

	public void EndLoadingPlaque() {

	}

	public void UpdateScreen() {
		if (DrawLoading) {
			EngineVGui?.OnLevelLoadingFinished();
		}
		else if (Sys.ExtendedError) {
			EngineVGui?.ShowErrorMessage();
		}

		DisabledForLoading = false;
		DrawLoading = false;
	}

	public void CenterPrint(ReadOnlySpan<char> str) { }
	public void CenterStringOff() { }
}
