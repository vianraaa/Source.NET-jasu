using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source.Common.GUI;
public enum CursorCode : byte
{
	User,
	None,
	Arrow,
	IBeam,
	Hourglass,
	WaitArrow,
	Crosshair,
	Up,
	SizeNWSE,
	SizeNESW,
	SizeWE,
	SizeNS,
	SizeAll,
	No,
	Hand,
	Blank,
	Last,
	AlwaysVisiblePush,
	AlwaysVisiblePop
}

public interface ICursor
{
	void Activate();
}