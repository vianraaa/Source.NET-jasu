using Microsoft.Extensions.DependencyInjection;

using SDL;

using Source.Common.Launcher;
using Source.Common.MaterialSystem;

namespace Source.SDLManager;

public unsafe class SDL3_LauncherManager(IServiceProvider services) : ILauncherManager
{
	SDL3_Window window;
	public nint CreateExtraContext() {
		return 0;
	}
	nint graphicsHandle;
	public bool CreateGameWindow(string title, bool windowed, int width, int height) {
		IMaterialSystem materials = services.GetRequiredService<IMaterialSystem>();
		SDL_WindowFlags flags = 0;
		flags |= SDL_WindowFlags.SDL_WINDOW_OPENGL;

		SDL3.SDL_GL_SetAttribute(SDL_GLAttr.SDL_GL_RED_SIZE, 8);
		SDL3.SDL_GL_SetAttribute(SDL_GLAttr.SDL_GL_GREEN_SIZE, 8);
		SDL3.SDL_GL_SetAttribute(SDL_GLAttr.SDL_GL_BLUE_SIZE, 8);
		SDL3.SDL_GL_SetAttribute(SDL_GLAttr.SDL_GL_ALPHA_SIZE, 8);
		SDL3.SDL_GL_SetAttribute(SDL_GLAttr.SDL_GL_STENCIL_SIZE, 8);
		SDL3.SDL_GL_SetAttribute(SDL_GLAttr.SDL_GL_CONTEXT_MAJOR_VERSION, 3);
		SDL3.SDL_GL_SetAttribute(SDL_GLAttr.SDL_GL_CONTEXT_MINOR_VERSION, 3);
		SDL3.SDL_GL_SetAttribute(SDL_GLAttr.SDL_GL_CONTEXT_PROFILE_MASK, SDL3.SDL_GL_CONTEXT_PROFILE_CORE);

		window = new SDL3_Window(services).Create(title, width, height, flags);
		graphicsHandle = (nint)SDL3.SDL_GL_CreateContext(window.HardwareHandle);

		if (graphicsHandle == 0) {
			Dbg.Error($"Could not create graphics! (SDL3 Says: {SDL3.SDL_GetError()})\n");
			return false;
		}

		// Set up the current context for materialsystem
		SDL3.SDL_GL_MakeCurrent(window.HardwareHandle, (SDL_GLContextState*)graphicsHandle);
		SDL3.SDL_GL_SetSwapInterval(0);

		if (!materials.InitializeGraphics(graphicsHandle, window.Width, window.Height)) {
			Dbg.Error($"Could not set graphics context!\n");
			return false;
		}

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
		return graphicsHandle;
	}

	public nint GetMainContext() {
		return graphicsHandle;
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

	public nint GetWindowHandle() => (nint)window.HardwareHandle;
	public void PumpWindowsMessageLoop() => window.PumpMessages();
	public int GetEvents(WindowEvent[] eventBuffer, int length) => window.GetEvents(eventBuffer, length);
}
