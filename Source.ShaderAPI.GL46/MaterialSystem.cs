using Microsoft.Extensions.DependencyInjection;

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

		Dbg._SpewMessage((TraceLogLevel)logLevel switch {
			TraceLogLevel.Info => SpewType.Message,
			TraceLogLevel.Trace => SpewType.Log,
			TraceLogLevel.Debug => SpewType.Message,
			TraceLogLevel.Warning => SpewType.Warning,
			TraceLogLevel.Error => SpewType.Warning,
			TraceLogLevel.Fatal => SpewType.Error,
			_ => SpewType.Message,
		}, "raylib", 1, new Color(255, 255, 255), message + "\n");
	}

	public unsafe bool InitializeGraphics(nint graphics, delegate* unmanaged[Cdecl]<byte*, void*> loadExts, int width, int height) {
		this.graphics = graphics;

		// Making a spew group just for Raylib.
		Dbg.SpewActivate("raylib", 1);
		Raylib.SetTraceLogCallback(&raylibSpew);

		Rlgl.LoadExtensions(loadExts);
		Rlgl.GlInit(width, height);

		Rlgl.Viewport(0, 0, width, height);
		Rlgl.MatrixMode(MatrixMode.Projection);
		Rlgl.LoadIdentity();
		Rlgl.Ortho(0, width, height, 0, 0, 1);
		Rlgl.MatrixMode(MatrixMode.ModelView);
		Rlgl.LoadIdentity();
		Rlgl.ClearColor(255, 255, 255, 255);
		Rlgl.EnableDepthTest();

		Rlgl.ClearScreenBuffers();
		return true;
	}

	public unsafe void BeginFrame(double frameTime) {
		Rlgl.LoadIdentity();
		var mfx = Raymath.MatrixToFloatV(GetScreenMatrix());
		Rlgl.MultMatrixf(mfx.v);

		Rlgl.ClearColor(255, 0, 0, 255);
		Rlgl.ClearScreenBuffers();
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
		Raylib.SwapScreenBuffer();
	}
}