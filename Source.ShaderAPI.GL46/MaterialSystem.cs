using CommunityToolkit.HighPerformance;

using Microsoft.Extensions.DependencyInjection;

using OpenGL;

using Raylib_cs;

using Source.Common;
using Source.Common.Formats.Keyvalues;
using Source.Common.GUI;
using Source.Common.Launcher;
using Source.Common.MaterialSystem;
using Source.Common.Utilities;

using System.Numerics;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Source.MaterialSystem;

public class MaterialSystem : IMaterialSystem
{
	MaterialDict Dict = [];
	nint graphics;
	public static void DLLInit(IServiceCollection services) {
		services.AddSingleton(x => (x.GetRequiredService<IMaterialSystem>() as MaterialSystem)!);
		services.AddSingleton<ISurface, MatSystemSurface>();
		services.AddSingleton<ITextureManager, TextureManager>();
	}

	readonly IServiceProvider services;
	public MaterialSystem(IServiceProvider services) {
		this.services = services;
	}

	ILauncherManager launcherMgr;

	public void ModInit() {
		launcherMgr = services.GetRequiredService<ILauncherManager>();
		matContext = new(() => new(this));
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

	ThreadLocal<MatRenderContext> matContext;
	public IMatRenderContext GetRenderContext() => matContext.Value;

	public bool SetMode(nint window, MaterialSystemConfig config) {
		int width = config.Width;
		int height = config.Height;
		launcherMgr.RenderedSize(true, ref width, ref height);
		return true;
	}

	public IMaterial CreateMaterial(string materialName, KeyValues keyValues) {
		IMaterialInternal material;
		lock (this) {
			material = new Material(materialName, TextureGroup.Other, keyValues);
		}

		AddMaterialToMaterialList(material);
		return material;
	}

	private void AddMaterialToMaterialList(IMaterialInternal material) {
		Dict[material] = new() {
			material = material,
			symbol = material.GetName().GetHashCode(),
			manuallyCreated = material.IsManuallyCreated()
		};
	}
}

public enum MatrixStackFlags : uint
{
	Dirty = 1 << 0,
	Identity = 1 << 1
}
public struct MatrixStackItem
{
	public Matrix4x4 Matrix;
	public MatrixStackFlags Flags;
}

public struct RenderTargetStackElement {
	public ITexture[] RenderTargets;
	public ITexture DepthTexture;

	public int ViewX;
	public int ViewY;
	public int ViewW;
	public int ViewH;
}

public class MatRenderContext : IMatRenderContext
{
	readonly IMaterialSystem materials;

	public MatRenderContext(IMaterialSystem materials) {
		this.materials = materials;
		RenderTargetStack = new RefStack<RenderTargetStackElement>();
		MatrixStacks = new RefStack<MatrixStackItem>[(int)MaterialMatrixMode.Count];
		for (int i = 0; i < MatrixStacks.Length; i++) {
			MatrixStacks[i] = new();
		}
	}
	RefStack<RenderTargetStackElement> RenderTargetStack;
	RefStack<MatrixStackItem>[] MatrixStacks;
	MaterialMatrixMode matrixMode;
	bool dirtyViewState;
	bool dirtyViewProjState;
	public ref MatrixStackItem CurMatrixItem => ref MatrixStacks[(int)matrixMode].Peek();
	public ref RenderTargetStackElement CurRenderTargetStack => ref RenderTargetStack.Peek();

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

	public void DepthRange(double near, double far) {
		Gl46.glDepthRange(near, far);
	}

	public void EndRender() {

	}

	public void Flush(bool flushHardware) {

	}

	public void GetViewport(out int x, out int y, out int width, out int height) {
		x = y = width = height = 0;
	}

	public void LoadIdentity() {
		ref MatrixStackItem item = ref CurMatrixItem;
		item.Matrix = Matrix4x4.Identity;
		item.Flags = MatrixStackFlags.Dirty | MatrixStackFlags.Identity;
			CurrentMatrixChanged();
	}

	public void MatrixMode(MaterialMatrixMode mode) {
		matrixMode = mode;
	}

	public void PushMatrix() {
		RefStack<MatrixStackItem> curStack = MatrixStacks[(int)matrixMode];
		curStack.Push(CurMatrixItem);
		CurrentMatrixChanged();
	}

	private void CurrentMatrixChanged() {
		if (matrixMode == MaterialMatrixMode.View) {
			dirtyViewState = true;
			dirtyViewProjState = true;
		}
		else if (matrixMode == MaterialMatrixMode.Projection) {
			dirtyViewProjState = true;
		}
	}

	public void Viewport(int x, int y, int width, int height) {
		Dbg.Assert(RenderTargetStack.Count > 0);

		RenderTargetStackElement newTOS = new();
		newTOS = CurRenderTargetStack; // copy
		newTOS.ViewX = x;
		newTOS.ViewY = y;
		newTOS.ViewW = width;
		newTOS.ViewH = height;
		RenderTargetStack.Pop();
		RenderTargetStack.Push(newTOS);
	}
}