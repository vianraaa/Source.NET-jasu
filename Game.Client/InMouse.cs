using Source.Common.Client;
using Source.Common.Commands;
using Source.Common.GUI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Client;

public partial class Input
{
	IVGuiInput? _input;
	IVGuiInput input => _input ??= Singleton<IVGuiInput>();



	ConVar cl_mouselook = new("1", FCvar.Archive, "Set to 1 to use mouse for look, 0 for keyboard look." );

	public void GetWindowCenter(out int x, out int y) {
		engine.GetScreenSize(out int w, out int h);
		x = w >> 1;
		y = h >> 1;
	}

	public void ResetMouse() {
		GetWindowCenter(out int x, out int y);
		SetMousePos(x, y);
	}

	private void SetMousePos(int x, int y) {
		input.SetCursorPos(x, y);
	}
}
