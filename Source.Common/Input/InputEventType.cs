namespace Source.Common.Input;

public enum InputEventType
{
	IE_ButtonPressed = 0,
	IE_ButtonReleased,
	IE_ButtonDoubleClicked,
	IE_AnalogValueChanged,

	FirstSystemEvent = 100,
	System_Quit = FirstSystemEvent,

	FirstGuiEvent = 1000,
	// Extensions of VguiInputEventType
	Gui_Close = FirstGuiEvent,
	Gui_LocateMouseClick,
	Gui_SetCursor,
	Gui_KeyTyped,
	Gui_KeyCodeTyped,
	Gui_InputLanguageChanged,
	Gui_IMESetWindow,
	Gui_IMEStartComposition,
	Gui_IMEComposition,
	Gui_IMEEndComposition,
	Gui_IMEShowCandidates,
	Gui_IMEChangeCandidates,
	Gui_IMECloseCandidates,
	Gui_IMERecomputeModes,

	FirstAppEvent = 2000,
	// Extensions of GameInputEventType
	App_Close = FirstAppEvent,
	App_WindowMove,
	App_AppActivated
}