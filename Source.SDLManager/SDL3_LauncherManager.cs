using Microsoft.Extensions.DependencyInjection;

using SDL;

using Source.Common.Launcher;
using Source.Common.MaterialSystem;
using Source.Common.ShaderAPI;

using System.Runtime.InteropServices;

namespace Source.SDLManager;

internal static class SDL3_State {
	static bool ready = false;
	internal static void InitializeIfRequired() {
		if (ready) return;

		SDL3.SDL_SetAppMetadata(Path.GetFileNameWithoutExtension(Environment.ProcessPath), "N/A", "N/A");
		if (!SDL3.SDL_InitSubSystem(SDL_InitFlags.SDL_INIT_VIDEO))
			throw new Exception("Couldn't initialize SDL3's video subsystem.");
		if (!SDL3.SDL_InitSubSystem(SDL_InitFlags.SDL_INIT_AUDIO))
			throw new Exception("Couldn't initialize SDL3's audio subsystem.");

		ready = true;
	}
}

public unsafe class SDL3_OpenGL46_Context(nint window, nint ctx) : IGraphicsContext
{
	public nint HardwareHandle => ctx;

	public void MakeCurrent() {
		SDL3.SDL_GL_MakeCurrent((SDL_Window*)window, (SDL_GLContextState*)ctx);
	}

	public void SetSwapInterval(float swapInterval) {
		SDL3.SDL_GL_SetSwapInterval((int)swapInterval);
	}

	public void SwapBuffers() {
		SDL3.SDL_GL_SwapWindow((SDL_Window*)window);
	}
}

public unsafe class SDL3_LauncherManager : ILauncherManager, IGraphicsProvider
{
	readonly IServiceProvider services;
	public SDL3_LauncherManager(IServiceProvider services) {
		this.services = services;
		SDL3_State.InitializeIfRequired();
	}
	SDL3_Window window;
	public unsafe bool CreateGameWindow(string title, bool windowed, int width, int height) {
		IMaterialSystem materials = services.GetRequiredService<IMaterialSystem>();
		SDL_WindowFlags flags = 0;
		flags |= SDL_WindowFlags.SDL_WINDOW_OPENGL;
		window = new SDL3_Window(services).Create(title, width, height, flags);

		return true;
	}

	[UnmanagedCallersOnly(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
	unsafe static void* GL_ProcAddress(byte* proc) {
		return (void*)SDL3.SDL_GL_GetProcAddress(proc);
	}

	public void DestroyGameWindow() {

	}

	public void DisplayedSize(out int width, out int height) {
		int w, h;
		SDL3.SDL_GetWindowSize(window.HardwareHandle, &w, &h);
		width = w;
		height = h;
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
	int renderedWidth, renderedHeight;
	public void RenderedSize(bool set, ref int width, ref int height) {
		if (set) {
			renderedWidth = width;
			renderedHeight = height;
		}
		else {
			width = renderedWidth;
			height = renderedHeight;
		}
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

	public IWindow GetWindow() => window;
	public void PumpWindowsMessageLoop() => window?.PumpMessages();
	public int GetEvents(WindowEvent[] eventBuffer, int length) => window.GetEvents(eventBuffer, length);

	public void CenterWindow(int v2, int v3) {

	}

	public bool PrepareContext(GraphicsDriver driver) {
		if (driver.HasFlag(GraphicsDriver.OpenGL)) {
			switch ((driver & ~GraphicsDriver.OpenGL)) {
				default:
					Error("Cannot support this OpenGL version");
					return false;
				case (GraphicsDriver)460: {
						if (!SDL3.SDL_GL_SetAttribute(SDL_GLAttr.SDL_GL_CONTEXT_MAJOR_VERSION, 4))
							Error($"GL Context: Bad major version... {SDL3.SDL_GetError()}");
						if (!SDL3.SDL_GL_SetAttribute(SDL_GLAttr.SDL_GL_CONTEXT_MINOR_VERSION, 6))
							Error($"GL Context: Bad minor version... {SDL3.SDL_GetError()}");
					}
					break;
			}

			if (!SDL3.SDL_GL_SetAttribute(SDL_GLAttr.SDL_GL_DEPTH_SIZE, 24))
				Error("GL Context: Bad depth request...");
			if (!SDL3.SDL_GL_SetAttribute(SDL_GLAttr.SDL_GL_STENCIL_SIZE, 8))
				Error("GL Context: Bad stencil request...");
			if (!SDL3.SDL_GL_SetAttribute(SDL_GLAttr.SDL_GL_DOUBLEBUFFER, 1))
				Error("GL Context: Can't double buffer?");

			return true;
		}
		return false;
	}

	public IGraphicsContext? CreateContext(in ShaderDeviceInfo deviceInfo, IWindow? window = null) {
		IGraphicsContext? gfx = null;

		window = window == null ? this.window : window;
		if (deviceInfo.Driver.HasFlag(GraphicsDriver.OpenGL)) {
			nint handle = (nint)((SDL3_Window)window).HardwareHandle;
			nint ctx = (nint)SDL3.SDL_GL_CreateContext((SDL_Window*)handle);
			gfx = new SDL3_OpenGL46_Context(handle, ctx);
			return gfx;
		}

		Warning("Cannot support this graphics API\n");
		return null;
	}

	public delegate* unmanaged[Cdecl]<byte*, void*> GL_LoadExtensionsPtr() => &GL_ProcAddress;
}
