using Microsoft.Extensions.DependencyInjection;

using OpenGL;

using Raylib_cs;

using Source.Common.GUI;
using Source.Common.Launcher;
using Source.Common.MaterialSystem;

using System.Numerics;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Source.MaterialSystem;

public class MaterialSystem(IServiceProvider services) : IMaterialSystem
{
	nint graphics;
	public static void DLLInit(IServiceCollection services) {
		services.AddSingleton(x => (x.GetRequiredService<IMaterialSystem>() as MaterialSystem)!);
		services.AddSingleton<ISurface, MatSystemSurface>();
	}

	ILauncherManager launcherMgr;

	public void ModInit() {
		launcherMgr = services.GetRequiredService<ILauncherManager>();
		matContext = new(this);
	}

	public bool SetMode(in MaterialSystemConfig config) {

		// For now, communicate to launcherMgr our desired display mode
		// as the current screen mode. More magic later for SetMode, when the time comes
		int width = config.Width, height = config.Height;
		launcherMgr.RenderedSize(true, ref width, ref height);

		return true;
	}

	public void ModShutdown() {

	}

	[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
	unsafe static void raylibSpew(int logLevel, sbyte* text, sbyte* args) {
		var message = Logging.GetLogMessage((nint)text, (nint)args);

		/*Dbg._SpewMessage((TraceLogLevel)logLevel switch {
			TraceLogLevel.Info => SpewType.Message,
			TraceLogLevel.Trace => SpewType.Log,
			TraceLogLevel.Debug => SpewType.Message,
			TraceLogLevel.Warning => SpewType.Warning,
			TraceLogLevel.Error => SpewType.Warning,
			TraceLogLevel.Fatal => SpewType.Error,
			_ => SpewType.Message,
		}, "raylib", 1, new Color(255, 255, 255), message + "\n");*/
	}

	public unsafe bool InitializeGraphics(nint graphics, delegate* unmanaged[Cdecl]<byte*, void*> loadExts, int width, int height) {
		this.graphics = graphics;

		// Making a spew group just for Raylib.
		Dbg.SpewActivate("raylib", 1);
		Raylib.SetTraceLogCallback(&raylibSpew);

		Gl46.Import((name) => (nint)loadExts((byte*)new Utf8Buffer(name).AsPointer()));
		Rlgl.LoadExtensions(loadExts);
		Rlgl.GlInit(width, height);

		Rlgl.Viewport(0, 0, width, height);
		Rlgl.MatrixMode(MatrixMode.Projection);
		Rlgl.LoadIdentity();
		Rlgl.Ortho(0, width, height, 0, 0, 1);
		Rlgl.MatrixMode(MatrixMode.ModelView);
		Rlgl.LoadIdentity();
		Gl46.glClearColor(0, 0, 0, 1);
		Gl46.glClear(Gl46.GL_COLOR_BUFFER_BIT | Gl46.GL_DEPTH_BUFFER_BIT);
		Rlgl.EnableDepthTest();

		Rlgl.ClearScreenBuffers();
		return true;
	}

	public unsafe void BeginFrame(double frameTime) {
		Rlgl.LoadIdentity();
		var mfx = Raymath.MatrixToFloatV(GetScreenMatrix());
		Rlgl.MultMatrixf(mfx.v);
	}

	private Matrix4x4 GetScreenMatrix() {
		launcherMgr.DisplayedSize(out int screenWidth, out _);
		int renderWidth = 0, renderHeight = 0;
		launcherMgr.RenderedSize(false, ref renderWidth, ref renderHeight);
		float scaleRatio = (float)renderWidth / (float)screenWidth;
		return Raymath.MatrixScale(scaleRatio, scaleRatio, 1);
	}

	public void EndFrame() {
		Rlgl.DrawRenderBatchActive();

		SwapBuffers();
	}

	public void SwapBuffers() {
		launcherMgr.Swap();
	}

	MatRenderContext matContext;
	public IMatRenderContext GetRenderContext() => matContext!;
}

public class MatRenderContext(MaterialSystem materials) : IMatRenderContext
{
	public void BeginRender() {
		
	}

	public void ClearBuffers(bool clearColor, bool clearDepth, bool clearStencil = false) {
		uint flags = 0;

		if (clearColor) 
			flags |= Gl46.GL_COLOR_BUFFER_BIT;
		if (clearColor) 
			flags |= Gl46.GL_DEPTH_BUFFER_BIT;
		if (clearColor) 
			flags |= Gl46.GL_STENCIL_BUFFER_BIT;

		Gl46.glClear(flags);
	}

	public void ClearColor3ub(byte r, byte g, byte b) {
		Gl46.glClearColor(r / 255f, g / 255f, b / 255f, 1);
	}

	public void ClearColor4ub(byte r, byte g, byte b, byte a) {
		Gl46.glClearColor(r / 255f, g / 255f, b / 255f, a / 255f);
	}

	public void EndRender() {
		
	}

	public void Flush(bool flushHardware) {
		
	}

	public void GetViewport(out int x, out int y, out int width, out int height) {
		x = y = width = height = 0;
	}

	public void Viewport(int x, int y, int width, int height) {
		
	}
}