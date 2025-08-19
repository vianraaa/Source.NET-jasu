using CommunityToolkit.HighPerformance;

using Microsoft.Extensions.DependencyInjection;

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
		services.AddSingleton<ISurface, MatSystemSurface>();
		services.AddSingleton<ITextureManager>(x => ((MaterialSystem)x.GetRequiredService<IMaterialSystem>()).TextureSystem);
		services.AddSingleton<IShaderSystem>(x => ((MaterialSystem)x.GetRequiredService<IMaterialSystem>()).ShaderSystem);
	}

	readonly IServiceProvider services;

	public TextureManager TextureSystem;
	public ShaderManager ShaderSystem;

	public MaterialSystem(IServiceProvider services) {
		this.services = services;
		TextureSystem = new();
		ShaderSystem = new() {
			Services = services
		};
		ShaderSystem.LoadAllShaderDLLs();
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

		Rlgl.LoadExtensions(loadExts);
		Rlgl.GlInit(width, height);

		Rlgl.Viewport(0, 0, width, height);
		Rlgl.MatrixMode(MatrixMode.Projection);
		Rlgl.LoadIdentity();
		Rlgl.Ortho(0, width, height, 0, 0, 1);
		Rlgl.MatrixMode(MatrixMode.ModelView);
		Rlgl.LoadIdentity();
		Rlgl.ClearColor(0, 0, 0, 1);
		Rlgl.ClearScreenBuffers();
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
	public IMatRenderContext GetRenderContext() => matContext!.Value!;

	public bool SetMode(nint window, MaterialSystemConfig config) {
		int width = config.Width;
		int height = config.Height;
		launcherMgr.RenderedSize(true, ref width, ref height);
		return true;
	}

	public IMaterial CreateMaterial(string materialName, KeyValues keyValues) {
		IMaterialInternal material;
		lock (this) {
			material = new Material(this, materialName, TextureGroup.Other, keyValues);
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

	public IMaterial? GetCurrentMaterial() {
		return GetRenderContext().GetCurrentMaterial();
	}

	public IMaterialInternal errorMaterial;
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

public struct RenderTargetStackElement
{
	public ITexture?[] RenderTargets;
	public ITexture? DepthTexture;

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
			ref MatrixStackItem item = ref MatrixStacks[i].Push();
			item.Matrix = Matrix4x4.Identity;
			item.Flags = MatrixStackFlags.Dirty | MatrixStackFlags.Identity;
		}
		RenderTargetStackElement initialElement = new() {
			RenderTargets = [null, null, null, null],
			DepthTexture = null,
			ViewX = 0,
			ViewY = 0,
			ViewW = -1,
			ViewH = -1
		};
		RenderTargetStack.Push(initialElement);
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

	public void ClearBuffers(bool clearColor, bool clearDepth, bool clearStencil = false) => Rlgl.ClearScreenBuffers(clearColor, clearDepth, clearStencil);
	public void ClearColor3ub(byte r, byte g, byte b) {
		Rlgl.ClearColor(r, g, b, 255);
	}

	public void ClearColor4ub(byte r, byte g, byte b, byte a) {
		Rlgl.ClearColor(r, g, b, a);
	}

	public void DepthRange(double near, double far) {
		Rlgl.DepthRange(near, far);
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

	IMaterialInternal? currentMaterial;

	public void Bind(IMaterial iMaterial, object? proxyData) {
		IMaterialInternal material = (IMaterialInternal)iMaterial;
		// material = material.GetRealTimeVersion(); // TODO: figure out how to do this.
		if (material == null) {
			Dbg.Warning("Programming error: MatRenderContext.Bind NULL material\n");
			material = ((MaterialSystem)materials).errorMaterial;
		}

		if (GetCurrentMaterialInternal() != material) {
			if (!material.IsPrecached()) {
				material.Precache();
			}
			SetCurrentMaterialInternal(material);
		}
	}

	private IMaterialInternal? GetCurrentMaterialInternal() {
		return currentMaterial;
	}
	private void SetCurrentMaterialInternal(IMaterialInternal? mat) {
		currentMaterial = mat;
	}

	public IMaterial? GetCurrentMaterial() {
		return currentMaterial;
	}
}