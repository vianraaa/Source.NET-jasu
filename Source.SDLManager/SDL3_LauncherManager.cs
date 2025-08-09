using SDL;

using Source.Common.Launcher;

namespace Source.SDLManager;

public unsafe class SDL3_LauncherManager(IServiceProvider services) : ILauncherManager
{
	SDL3_Window window;
	public nint CreateExtraContext() {
		return 0;
	}

	public bool CreateGameWindow(string title, bool windowed, int width, int height) {
		SDL_WindowFlags flags = 0;

		window = new SDL3_Window(services).Create(title, width, height, flags);
		return true;
	}

	public void DeleteContext(nint context) {

	}

	public void DestroyGameWindow() {

	}

	public void DisplayedSize(out uint width, out uint height) {
		width = height = 0;
	}

	public nint GetGLContextForWindow(nint windowref) {
		return 0;
	}

	public nint GetMainContext() {
		return 0;
	}

	public void GetMouseDelta(out int x, out int y, bool ignoreNextMouseDelta = false) {
		throw new NotImplementedException();
	}

	public void GetNativeDisplayInfo(int nDisplay, out uint width, out uint height, out uint refreshHz) {
		throw new NotImplementedException();
	}

	public bool IsWindowFullScreen() {
		throw new NotImplementedException();
	}

	public bool MakeContextCurrent(nint context) {
		return false;
	}

	public void MoveWindow(int x, int y) {

	}
	public void RenderedSize(ref uint width, ref uint height, bool set) {
		throw new NotImplementedException();
	}

	public void SetApplicationIcon(ReadOnlySpan<char> appIconFile) {
		throw new NotImplementedException();
	}

	public void SetCursorPosition(int x, int y) {
		throw new NotImplementedException();
	}

	public void SetWindowFullScreen(bool fullscreen, int width, int height) {
		throw new NotImplementedException();
	}

	public void SizeWindow(int width, int tall) {
		throw new NotImplementedException();
	}

	public nint GetWindowHandle() => (nint)window.GetSDLWindowHandle();
	public void PumpWindowsMessageLoop() => window.PumpMessages();
	public int GetEvents(WindowEvent[] eventBuffer, int length) => window.GetEvents(eventBuffer, length);
}
