using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source.Common.Engine;

public interface IGame
{
	bool CreateGameWindow();
	void DestroyGameWindow();
	void SetGameWindow(nint hWnd);
	bool InputAttachToGameWindow();
	void InputDetachFromGameWindow();

	void PlayStartupVideos();
	nint GetMainWindow();
	nint GetMainDeviceWindow();

	nint GetMainWindowAddress();
	void GetDesktopInfo(out int width, out int height, out int refreshrate);

	void SetWindowXY(int x, int y);
	void SetWindowSize(int w, int h);

	void GetWindowRect(out int x, out int y, out int w, out int h);

	// Not Alt-Tabbed away
	bool IsActiveApp();

	void DispatchAllStoredGameMessages();
}
