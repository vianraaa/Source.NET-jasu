namespace Source.Common.GUI;

public interface ICursor {
	public void Activate();
}

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
