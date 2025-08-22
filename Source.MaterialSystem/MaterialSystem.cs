using CommunityToolkit.HighPerformance;

using Microsoft.Extensions.DependencyInjection;

using Raylib_cs;

using Source.Common;
using Source.Common.Filesystem;
using Source.Common.Formats.Keyvalues;
using Source.Common.GUI;
using Source.Common.Launcher;
using Source.Common.MaterialSystem;
using Source.Common.ShaderAPI;
using Source.Common.Utilities;

using System.Diagnostics;
using System.Numerics;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Source.MaterialSystem;

public class MaterialSystem : IMaterialSystem, IShaderUtil
{
	MaterialDict Dict = [];
	nint graphics;
	public static void DLLInit(IServiceCollection services) {
		services.AddSingleton<ISurface, MatSystemSurface>();
		services.AddSingleton<IShaderAPI, ShaderAPIGl46>();
		services.AddSingleton<IShaderShadow, ShaderShadowGl46>();
		services.AddSingleton<IShaderUtil>(x => x.GetRequiredService<MaterialSystem>());
		services.AddSingleton<ITextureManager, TextureManager>();
		services.AddSingleton<IShaderSystem, ShaderSystem>();
		services.AddSingleton<IMaterialSystemHardwareConfig, HardwareConfig>();
		services.AddSingleton<MeshMgr>();
	}

	readonly IServiceProvider services;

	public readonly IFileSystem FileSystem;
	public readonly TextureManager TextureSystem;
	public readonly ShaderSystem ShaderSystem;
	public readonly ShaderAPIGl46 ShaderAPI;
	public readonly ShaderShadowGl46 ShaderShadow;
	public readonly MeshMgr MeshMgr;
	public readonly HardwareConfig HardwareConfig;

	public MaterialSystem(IServiceProvider services) {
		this.services = services;

		FileSystem = services.GetRequiredService<IFileSystem>();
		ShaderAPI = (services.GetRequiredService<IShaderAPI>() as ShaderAPIGl46)!;
		ShaderShadow = (services.GetRequiredService<IShaderShadow>() as ShaderShadowGl46)!;
		TextureSystem = (services.GetRequiredService<ITextureManager>() as TextureManager)!;
		MeshMgr = (services.GetRequiredService<MeshMgr>() as MeshMgr)!; // todo: interface
		HardwareConfig = (services.GetRequiredService<IMaterialSystemHardwareConfig>() as HardwareConfig)!; // todo: interface
		ShaderSystem = (services.GetRequiredService<IShaderSystem>() as ShaderSystem)!;

		// Link up
		ShaderAPI.TransitionTable = new(ShaderShadow);
		ShaderShadow.MeshMgr = MeshMgr;

		ShaderAPI.MeshMgr = MeshMgr;
		TextureSystem.MaterialSystem = this;
		MeshMgr.Materials = this;
		ShaderSystem.Services = services;
		ShaderSystem.materials = this;
		ShaderSystem.shaderAPI = ShaderAPI;
		ShaderShadow.HardwareConfig = HardwareConfig;
		MeshMgr.ShaderAPI = ShaderAPI;

		MeshMgr.Init();

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

	public bool CanUseEditorMaterials() {
		return false; //todo
	}

	public bool IsInStubMode() => false;

	public bool OnDrawMesh(IMesh mesh, int firstIndex, int indexCount) {
		if (IsInStubMode())
			return false;

		return GetRenderContextInternal().OnDrawMesh(mesh, firstIndex, indexCount);
	}

	public IMatRenderContextInternal GetRenderContextInternal() => matContext!.Value!;

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

public class MatRenderContext : IMatRenderContextInternal
{
	readonly MaterialSystem materials;
	readonly IShaderDynamicAPI shaderAPI;

	public MatRenderContext(MaterialSystem materials) {
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
		shaderAPI = materials.ShaderAPI;
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
		shaderAPI.PushMatrix();
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

	public void PopMatrix() {
		RefStack<MatrixStackItem> curStack = MatrixStacks[(int)matrixMode];
		curStack.Pop();
		CurrentMatrixChanged();
		shaderAPI.PopMatrix();
	}

	public IShaderAPI GetShaderAPI() {
		return materials.ShaderAPI;
	}

	public bool OnDrawMesh(IMesh mesh, int firstIndex, int indexCount) {
		// SyncMatrices();
		return true;
	}

	public IMesh GetDynamicMesh(bool buffered, IMesh? vertexOverride = null, IMesh? indexOverride = null, IMaterial? autoBind = null) {
		if(autoBind != null) {
			Bind(autoBind, null);
		}

		if (vertexOverride != null) {
			if (vertexOverride.GetVertexFormat().CompressionType() != VertexCompressionType.None) {
				// UNDONE: support compressed dynamic meshes if needed (pro: less VB memory, con: time spent compressing)
				Debugger.Break();
				return null;
			}
		}

		// For anything more than 1 bone, imply the last weight from the 1 - the sum of the others.
		int nCurrentBoneCount = shaderAPI.GetCurrentNumBones();
		Assert(nCurrentBoneCount <= 4);
		if (nCurrentBoneCount > 1) {
			--nCurrentBoneCount;
		}

		return shaderAPI.GetDynamicMesh(GetCurrentMaterialInternal()!, nCurrentBoneCount, buffered, vertexOverride, indexOverride);
	}
}