using Source.Common.Launcher;

namespace Source.Common.Input;

public interface IInputSystem
{
	public static IInputSystem? _;

	public void AttachToWindow(IWindow window);
	public void DetachFromWindow();

	public void EnableInput(bool enabled);
	public void EnableMessagePump(bool enabled);

	public void PollInputState();
	public int GetPollTick();
	public bool IsButtonDown(ButtonCode code);
	public int GetButtonPressedTick(ButtonCode code);
	public int GetButtonReleasedTick(ButtonCode code);
	public long GetEventCount();
	public IEnumerable<InputEvent> GetEventData();
	public void PostUserEvent(in InputEvent ev);
	public void ResetInputState() { }
	public void SetPrimaryUserId(int userId) { }

	public ButtonCode VirtualKeyToButtonCode(int virtualKey);
	public int ButtonCodeToVirtualKey(ButtonCode code);
	public ButtonCode ScanCodeToButtonCode(int param);

	public int GetPollCount();
	public void SetCursorPosition(int x, int y);
	public bool GetRawMouseAccumulators(out int accumX, out int accumY);
	public void SetConsoleTextMode(bool consoleTextMode);
}