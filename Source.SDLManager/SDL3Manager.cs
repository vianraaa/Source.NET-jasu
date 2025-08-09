using SDL;

using Source.Common.Launcher;

namespace Source.SDLManager;

public unsafe class SDL3Manager : ILauncherManager
{
	SDL_Window* hWindow;
	public nint CreateExtraContext() {
		throw new NotImplementedException();
	}

	public bool CreateGameWindow(string title, bool windowed, int width, int height) {
		SDL_WindowFlags flags = 0;

		hWindow = SDL3.SDL_CreateWindow(title, width, height, flags);
		if (hWindow == null)
			return false;
		return true;
	}

	public void DeleteContext(nint context) {
		throw new NotImplementedException();
	}

	public void DestroyGameWindow() {
		throw new NotImplementedException();
	}

	public void DisplayedSize(out uint width, out uint height) {
		throw new NotImplementedException();
	}

	public nint GetGLContextForWindow(nint windowref) {
		throw new NotImplementedException();
	}

	public nint GetMainContext() {
		throw new NotImplementedException();
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
		throw new NotImplementedException();
	}

	public void MoveWindow(int x, int y) {
		throw new NotImplementedException();
	}

	public void PumpWindowsMessageLoop() {
		throw new NotImplementedException();
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

	public nint GetWindowHandle() => (nint)hWindow;

	public int GetEvents(WindowEvent[] eventBuffer, int length) {
		throw new NotImplementedException();
	}
}
