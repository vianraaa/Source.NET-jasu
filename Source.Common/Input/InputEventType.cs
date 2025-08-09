namespace Source.Common.Input;

public enum InputEventType
{
	ButtonPressed = 0,
	ButtonReleased,
	ButtonDoubleClicked,
	AnalogValueChanged,

	FirstSystemEvent = 100,
	Quit = FirstSystemEvent,

	FirstGuiEvent = 1000,

	IE_KeyTyped = FirstGuiEvent + 3,
	KeyCodeTyped = FirstGuiEvent + 4,

	FirstAppEvent = 2000
}