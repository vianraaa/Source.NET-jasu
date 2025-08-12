using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source.Common.Engine;

public interface IVideoMode
{
	void DrawStartupGraphic();
	bool CreateGameWindow(int width, int height, bool windowed);
	void SetGameWindow(nint window);
	bool SetMode(int width, int height, bool windowed);
}