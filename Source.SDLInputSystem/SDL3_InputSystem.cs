
using Microsoft.Extensions.DependencyInjection;

using SDL;

using Source;
using Source.Common.Input;
using Source.Common.Launcher;

namespace Nucleus.SDL3Window;

public class InputState
{
	public bool[] ButtonState;
	public int[] ButtonPressedTick;
	public int[] ButtonReleasedTick;
	public Queue<InputEvent> Events;
	public bool Dirty;
	public InputState() {
		ButtonState = new bool[(int)ButtonCode.Count];
		ButtonPressedTick = new int[(int)ButtonCode.Count];
		ButtonReleasedTick = new int[(int)ButtonCode.Count];
		Events = [];
		Dirty = false;
	}
}

public class SDL3_InputSystem(IServiceProvider services) : IInputSystem
{
	public InputState[] InputStates = [new(), new()];

	InputState InputState => IsPolling ? InputStates[1] : InputStates[0];
	InputState QueuedInputState => InputStates[(int)InputStateType.Queued];
	InputState CurrentInputState => InputStates[(int)InputStateType.Current];

	public nint Window;
	public bool InputEnabled;
	public bool PumpEnabled;
	public bool IsPolling;

	int LastSampleTick;
	int LastPollTick;
	int PollCount;

	ILauncherManager launcherMgr;

	public void AttachToWindow(nint window) {
		Window = window;
		launcherMgr ??= services.GetRequiredService<ILauncherManager>();
		ClearInputState();
	}

	public int ButtonCodeToVirtualKey(ButtonCode code) {
		throw new NotImplementedException();
	}

	public void DetachFromWindow() {
		Window = 0;
	}

	public void EnableInput(bool enabled) => InputEnabled = enabled;
	public void EnableMessagePump(bool enabled) => PumpEnabled = enabled;

	public int GetButtonPressedTick(ButtonCode code) {
		throw new NotImplementedException();
	}

	public int GetButtonReleasedTick(ButtonCode code) {
		throw new NotImplementedException();
	}

	public long GetEventCount() => CurrentInputState.Events.Count;
	public IEnumerable<InputEvent> GetEventData() => CurrentInputState.Events;

	public int GetPollCount() {
		throw new NotImplementedException();
	}

	public int GetPollTick() {
		throw new NotImplementedException();
	}

	public bool GetRawMouseAccumulators(out int accumX, out int accumY) {
		throw new NotImplementedException();
	}

	public bool IsButtonDown(ButtonCode code) {
		throw new NotImplementedException();
	}

	public void ClearInputState() {
		foreach (var state in InputStates) {
			state.Events.Clear();
			Array.Clear(state.ButtonState);
			Array.Clear(state.ButtonPressedTick);
			Array.Clear(state.ButtonReleasedTick);
			state.Dirty = false;
		}
	}

	static ButtonCode[] scantokey;
	static SDL3_InputSystem() {
		scantokey = new ButtonCode[(int)SDL_Scancode.SDL_SCANCODE_COUNT];
		for (int i = (int)SDL_Scancode.SDL_SCANCODE_A; i <= (int)SDL_Scancode.SDL_SCANCODE_Z; i++)
			scantokey[i] = ButtonCode.KeyA + (i - (int)SDL_Scancode.SDL_SCANCODE_A);
		for (int i = (int)SDL_Scancode.SDL_SCANCODE_1; i <= (int)SDL_Scancode.SDL_SCANCODE_9; i++)
			scantokey[i] = ButtonCode.Key1 + (i - (int)SDL_Scancode.SDL_SCANCODE_1);
		for (int i = (int)SDL_Scancode.SDL_SCANCODE_F1; i <= (int)SDL_Scancode.SDL_SCANCODE_F12; i++)
			scantokey[i] = ButtonCode.KeyF1 + (i - (int)SDL_Scancode.SDL_SCANCODE_F1);
		for (int i = (int)SDL_Scancode.SDL_SCANCODE_KP_1; i <= (int)SDL_Scancode.SDL_SCANCODE_KP_9; i++)
			scantokey[i] = ButtonCode.KeyPad1 + (i - (int)SDL_Scancode.SDL_SCANCODE_KP_1);

		scantokey[(int)SDL_Scancode.SDL_SCANCODE_0] = ButtonCode.Key0;
		scantokey[(int)SDL_Scancode.SDL_SCANCODE_KP_0] = ButtonCode.KeyPad0;
		scantokey[(int)SDL_Scancode.SDL_SCANCODE_RETURN] = ButtonCode.KeyEnter;
		scantokey[(int)SDL_Scancode.SDL_SCANCODE_ESCAPE] = ButtonCode.KeyEscape;
		scantokey[(int)SDL_Scancode.SDL_SCANCODE_BACKSPACE] = ButtonCode.KeyBackspace;
		scantokey[(int)SDL_Scancode.SDL_SCANCODE_TAB] = ButtonCode.KeyTab;
		scantokey[(int)SDL_Scancode.SDL_SCANCODE_SPACE] = ButtonCode.KeySpace;
		scantokey[(int)SDL_Scancode.SDL_SCANCODE_MINUS] = ButtonCode.KeyMinus;
		scantokey[(int)SDL_Scancode.SDL_SCANCODE_EQUALS] = ButtonCode.KeyEqual;
		scantokey[(int)SDL_Scancode.SDL_SCANCODE_LEFTBRACKET] = ButtonCode.KeyLBracket;
		scantokey[(int)SDL_Scancode.SDL_SCANCODE_RIGHTBRACKET] = ButtonCode.KeyRBracket;
		scantokey[(int)SDL_Scancode.SDL_SCANCODE_BACKSLASH] = ButtonCode.KeyBackslash;
		scantokey[(int)SDL_Scancode.SDL_SCANCODE_SEMICOLON] = ButtonCode.KeySemicolon;
		scantokey[(int)SDL_Scancode.SDL_SCANCODE_APOSTROPHE] = ButtonCode.KeyApostrophe;
		scantokey[(int)SDL_Scancode.SDL_SCANCODE_GRAVE] = ButtonCode.KeyBackquote;
		scantokey[(int)SDL_Scancode.SDL_SCANCODE_COMMA] = ButtonCode.KeyComma;
		scantokey[(int)SDL_Scancode.SDL_SCANCODE_PERIOD] = ButtonCode.KeyPeriod;
		scantokey[(int)SDL_Scancode.SDL_SCANCODE_SLASH] = ButtonCode.KeySlash;
		scantokey[(int)SDL_Scancode.SDL_SCANCODE_CAPSLOCK] = ButtonCode.KeyCapsLock;
		scantokey[(int)SDL_Scancode.SDL_SCANCODE_SCROLLLOCK] = ButtonCode.KeyScrollLock;
		scantokey[(int)SDL_Scancode.SDL_SCANCODE_INSERT] = ButtonCode.KeyInsert;
		scantokey[(int)SDL_Scancode.SDL_SCANCODE_HOME] = ButtonCode.KeyHome;
		scantokey[(int)SDL_Scancode.SDL_SCANCODE_PAGEUP] = ButtonCode.KeyPageUp;
		scantokey[(int)SDL_Scancode.SDL_SCANCODE_DELETE] = ButtonCode.KeyDelete;
		scantokey[(int)SDL_Scancode.SDL_SCANCODE_END] = ButtonCode.KeyEnd;
		scantokey[(int)SDL_Scancode.SDL_SCANCODE_PAGEDOWN] = ButtonCode.KeyPageDown;
		scantokey[(int)SDL_Scancode.SDL_SCANCODE_RIGHT] = ButtonCode.KeyRight;
		scantokey[(int)SDL_Scancode.SDL_SCANCODE_LEFT] = ButtonCode.KeyLeft;
		scantokey[(int)SDL_Scancode.SDL_SCANCODE_DOWN] = ButtonCode.KeyDown;
		scantokey[(int)SDL_Scancode.SDL_SCANCODE_UP] = ButtonCode.KeyUp;
		scantokey[(int)SDL_Scancode.SDL_SCANCODE_NUMLOCKCLEAR] = ButtonCode.KeyNumLock;
		scantokey[(int)SDL_Scancode.SDL_SCANCODE_KP_DIVIDE] = ButtonCode.KeyPadDivide;
		scantokey[(int)SDL_Scancode.SDL_SCANCODE_KP_MULTIPLY] = ButtonCode.KeyPadMultiply;
		scantokey[(int)SDL_Scancode.SDL_SCANCODE_KP_MINUS] = ButtonCode.KeyPadMinus;
		scantokey[(int)SDL_Scancode.SDL_SCANCODE_KP_PLUS] = ButtonCode.KeyPadPlus;
		scantokey[(int)SDL_Scancode.SDL_SCANCODE_KP_ENTER] = ButtonCode.KeyEnter;
		scantokey[(int)SDL_Scancode.SDL_SCANCODE_KP_PERIOD] = ButtonCode.KeyPadDecimal;
		scantokey[(int)SDL_Scancode.SDL_SCANCODE_APPLICATION] = ButtonCode.KeyApp;
		scantokey[(int)SDL_Scancode.SDL_SCANCODE_LCTRL] = ButtonCode.KeyLControl;
		scantokey[(int)SDL_Scancode.SDL_SCANCODE_LSHIFT] = ButtonCode.KeyLShift;
		scantokey[(int)SDL_Scancode.SDL_SCANCODE_LALT] = ButtonCode.KeyLAlt;
		scantokey[(int)SDL_Scancode.SDL_SCANCODE_LGUI] = ButtonCode.KeyLWin;
		scantokey[(int)SDL_Scancode.SDL_SCANCODE_RCTRL] = ButtonCode.KeyRControl;
		scantokey[(int)SDL_Scancode.SDL_SCANCODE_RSHIFT] = ButtonCode.KeyRShift;
		scantokey[(int)SDL_Scancode.SDL_SCANCODE_RALT] = ButtonCode.KeyRAlt;
		scantokey[(int)SDL_Scancode.SDL_SCANCODE_RGUI] = ButtonCode.KeyRWin;
	}


	static bool MapVirtualKeyToButtonCode(int virtualKeyCode, out ButtonCode pOut) {
		if (virtualKeyCode < 0)
			pOut = (ButtonCode)(-1 * virtualKeyCode);
		else {
			virtualKeyCode &= 0x000000ff;
			pOut = (ButtonCode)scantokey[virtualKeyCode];
		}

		return true;
	}

	public void PostButtonPressedEvent(InputEventType type, int tick, ButtonCode scanCode, ButtonCode virtualCode) {
		InputState state = InputState;

		if (!state.ButtonState.IsBitSet((int)scanCode)) {
			state.ButtonState.Set((int)scanCode);
			state.ButtonPressedTick[(int)scanCode] = tick;

			PostEvent(type, tick, (int)scanCode, (int)virtualCode, 0);
		}
	}

	public void PostButtonReleasedEvent(InputEventType type, int tick, ButtonCode scanCode, ButtonCode virtualCode) {
		InputState state = InputState;

		if (state.ButtonState.IsBitSet((int)scanCode)) {
			state.ButtonState.Clear((int)scanCode);
			state.ButtonReleasedTick[(int)scanCode] = tick;

			PostEvent(type, tick, (int)scanCode, (int)virtualCode, 0);
		}
	}

	private enum InputStateType
	{
		Queued,
		Current,
		Count
	}

	WindowEvent[] eventBuffer = new WindowEvent[32];
	public void CopyInputState(InputState dest, InputState src, bool copyEvents) {
		dest.Events.Clear();
		dest.Dirty = false;
		if (src.Dirty) {
			dest.ButtonState = src.ButtonState;
			Array.ConstrainedCopy(src.ButtonPressedTick, 0, dest.ButtonPressedTick, 0, dest.ButtonPressedTick.Length);
			Array.ConstrainedCopy(src.ButtonReleasedTick, 0, dest.ButtonReleasedTick, 0, dest.ButtonReleasedTick.Length);
			if (copyEvents) {
				if (src.Events.Count > 0) {
					dest.Events.Clear();
					dest.Events.EnsureCapacity(src.Events.Count);
					foreach (var queued in src.Events) dest.Events.Enqueue(queued);
				}
			}
		}
	}
	public void PollInputState() {
		IsPolling = true;
		PollCount++;

		InputState queuedState = QueuedInputState;
		CopyInputState(CurrentInputState, queuedState, true);
		LastPollTick = LastSampleTick;

		PollInputState_Platform();
		CopyInputState(queuedState, CurrentInputState, false);
		IsPolling = false;
	}
	public void PollInputState_Platform() {
		while (true) {
			int events = launcherMgr?.GetEvents(eventBuffer, eventBuffer.Length) ?? 0;
			if (events == 0) break;
			for (int i = 0; i < events; i++) {
				WindowEvent ev = eventBuffer[i];
				switch (ev.EventType) {
					case WindowEventType.KeyDown: {
							if (MapVirtualKeyToButtonCode(ev.VirtualKeyCode, out ButtonCode virtualCode)) {
								ButtonCode scancode = virtualCode;
								if (scancode != ButtonCode.None)
									PostButtonPressedEvent(InputEventType.ButtonPressed, LastSampleTick, scancode, virtualCode);

								InputEvent newEv = new() {
									Tick = GetPollTick(),
									Type = InputEventType.FirstGuiEvent + 4,
									Data = (int)scancode
								};
								IInputSystem._!.PostUserEvent(newEv);
							}

							if (!ev.ModifierKeyMask.HasFlag(KeyModifier.Command) && ev.VirtualKeyCode >= 0 && ev.UTF8Key > 0) {
								InputEvent newEv = new() {
									Tick = GetPollTick(),
									Type = InputEventType.FirstGuiEvent + 3,
									Data = ev.UTF8Key
								};
								IInputSystem._!.PostUserEvent(newEv);
							}
						}
						break;

					case WindowEventType.KeyUp: {
							if (MapVirtualKeyToButtonCode(ev.VirtualKeyCode, out ButtonCode virtualCode)) {
								ButtonCode scancode = virtualCode;
								if (scancode != ButtonCode.None)
									PostButtonReleasedEvent(InputEventType.ButtonPressed, LastSampleTick, scancode, virtualCode);
							}
						}
						break;

					case WindowEventType.MouseButtonDown: {
							UpdateMouseButtonState(ev.MouseButtonFlags, ev.MouseClickCount > 1 ? ev.MouseButton switch {
								MouseButton.Right => ButtonCode.MouseRight,
								MouseButton.Middle => ButtonCode.MouseMiddle,
								MouseButton.Button4 => ButtonCode.Mouse4,
								MouseButton.Button5 => ButtonCode.Mouse5,
								MouseButton.Left or _ => ButtonCode.MouseLeft,
							} : ButtonCode.Invalid);
						}
						break;
					case WindowEventType.MouseButtonUp:
						UpdateMouseButtonState(ev.MouseButtonFlags);
						break;

					case WindowEventType.AppQuit:
						PostEvent(InputEventType.Quit, LastSampleTick, 0, 0, 0);
						break;
				}
			}
		}
	}

	private void UpdateMouseButtonState(MouseButton buttonMask, ButtonCode dblClickCode = ButtonCode.Invalid) {
		for (int i = 0; i < 5; ++i) {
			ButtonCode code = ButtonCode.MouseFirst + i;
			bool down = ((int)buttonMask & (1 << i)) != 0;
			if (down) {
				InputEventType type = code != dblClickCode ? InputEventType.ButtonPressed : InputEventType.ButtonDoubleClicked;
				PostButtonPressedEvent(type, LastSampleTick, code, code);
			}
			else
				PostButtonReleasedEvent(InputEventType.ButtonReleased, LastSampleTick, code, code);
		}
	}

	public void PostEvent(InputEventType type, int tick, int data, int data2, int data3) {
		InputEvent ev = new();
		ev.Type = type;
		ev.Tick = tick;
		ev.Data = data;
		ev.Data2 = data2;
		ev.Data3 = data3;
		PostUserEvent(ev);
	}

	public void PostUserEvent(InputEvent ev) {
		InputState state = InputState;
		state.Events.Enqueue(ev);
		state.Dirty = true;
	}

	public ButtonCode ScanCodeToButtonCode(int param) {
		throw new NotImplementedException();
	}

	public void SetConsoleTextMode(bool consoleTextMode) {
		throw new NotImplementedException();
	}

	public void SetCursorPosition(int x, int y) {
		throw new NotImplementedException();
	}

	public ButtonCode VirtualKeyToButtonCode(int virtualKey) {
		throw new NotImplementedException();
	}
}
