using Source.Common.Engine;
using Source.Common.Filesystem;
using Source.Common.Formats.Keyvalues;
using Source.Common.Launcher;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source.Engine;

public class Game(ILauncherManager? launcherManager, Sys Sys, IFileSystem fileSystem) : IGame
{
	public bool CreateGameWindow(int width, int height, bool windowed) {
		if (launcherManager == null) {
			Sys.Error("Tried to call Game.CreateGameWindow without a valid ILauncherManager implementation.");
			return false;
		}

		string windowName = "HALF-LIFE 2";
		{
			KeyValues modinfo = new();
			if (modinfo.LoadFromFile(fileSystem, "gameinfo.txt"))
				windowName = modinfo.GetString("game") ?? windowName;
		}

		Console.Title = windowName;
		return launcherManager.CreateGameWindow(windowName, windowed, width, height);
	}

	public void DestroyGameWindow() {
		throw new NotImplementedException();
	}

	public void DispatchAllStoredGameMessages() {
		throw new NotImplementedException();
	}

	public void GetDesktopInfo(out int width, out int height, out int refreshrate) {
		throw new NotImplementedException();
	}

	public nint GetMainDeviceWindow() {
		throw new NotImplementedException();
	}

	public nint GetMainWindow() {
		throw new NotImplementedException();
	}

	public nint GetMainWindowAddress() {
		throw new NotImplementedException();
	}

	public void GetWindowRect(out int x, out int y, out int w, out int h) {
		throw new NotImplementedException();
	}

	public bool InputAttachToGameWindow() {
		throw new NotImplementedException();
	}

	public void InputDetachFromGameWindow() {
		throw new NotImplementedException();
	}

	public bool IsActiveApp() {
		throw new NotImplementedException();
	}

	public void PlayStartupVideos() {
		throw new NotImplementedException();
	}

	public void SetGameWindow(nint hWnd) {
		throw new NotImplementedException();
	}

	public void SetWindowSize(int w, int h) {
		throw new NotImplementedException();
	}

	public void SetWindowXY(int x, int y) {
		throw new NotImplementedException();
	}
}
