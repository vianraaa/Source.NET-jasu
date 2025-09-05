using Source.Common.Commands;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source.Engine;

public class Con(ICvar cvar)
{
	public void Init() { }
	public void Shutdown() { }
	public void Execute() { }

	internal void ClearNotify() {

	}

	public void ColorPrintf(in Color clr, ReadOnlySpan<char> fmt) {
		cvar.ConsoleColorPrintf(in clr, fmt);
	}
}