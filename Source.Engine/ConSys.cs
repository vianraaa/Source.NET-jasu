using Source.Common.Commands;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source.Engine;

public class Con(ICvar cvar, IEngineVGui EngineVGui)
{
	public void Init() { }
	public void Shutdown() { }
	public void Execute() { }

	// TODO: ConPanel

	internal void ClearNotify() {

	}

	public void Clear() {
		Singleton<IEngineVGui>().ClearConsole();
		ClearNotify();
	}

	[ConCommand] void clear() => Clear();

	public void ColorPrintf(in Color clr, ReadOnlySpan<char> fmt) {
		cvar.ConsoleColorPrintf(in clr, fmt);
	}

	public bool IsVisible() => EngineVGui.IsConsoleVisible();
}