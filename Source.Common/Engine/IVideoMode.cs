using CommunityToolkit.HighPerformance;

using Source.Common.Engine;
using Source.Common.Utilities;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source.Common.Engine;


public struct ViewRect
{
	public int X, Y, Width, Height;
}

public interface IVideoMode
{
	void DrawStartupGraphic();
	bool CreateGameWindow(int width, int height, bool windowed);
	void SetGameWindow(nint window);
	bool SetMode(int width, int height, bool windowed);
	ViewRects GetClientViewRect();
}