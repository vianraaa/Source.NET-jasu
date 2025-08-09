namespace Source.Common.Launcher;

public enum WindowEventType
{
	KeyDown,
	KeyUp,
	MouseButtonDown,
	MouseMove,
	MouseButtonUp,
	AppActivate,
	MouseScroll,
	AppQuit,

	/// <summary>
	/// Event was one of the above, but has been handled and should be ignored now
	/// </summary>
	Deleted
}
