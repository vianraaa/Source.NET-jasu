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
		services.AddSingleton<MaterialSystem_Config>();
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
	public readonly MaterialSystem_Config Config;

	public MaterialSystem(IServiceProvider services) {
		this.services = services;

		FileSystem = services.GetRequiredService<IFileSystem>();
		ShaderAPI = (services.GetRequiredService<IShaderAPI>() as ShaderAPIGl46)!;
		ShaderShadow = (services.GetRequiredService<IShaderShadow>() as ShaderShadowGl46)!;
		TextureSystem = (services.GetRequiredService<ITextureManager>() as TextureManager)!;
		MeshMgr = (services.GetRequiredService<MeshMgr>() as MeshMgr)!; // todo: interface
		HardwareConfig = (services.GetRequiredService<IMaterialSystemHardwareConfig>() as HardwareConfig)!; // todo: interface
		ShaderSystem = (services.GetRequiredService<IShaderSystem>() as ShaderSystem)!;
		Config = services.GetRequiredService<MaterialSystem_Config>()!;

		// Link up
		ShaderAPI.TransitionTable = new(ShaderShadow);
		ShaderShadow.MeshMgr = MeshMgr;

		ShaderAPI.MeshMgr = MeshMgr;
		TextureSystem.MaterialSystem = this;
		MeshMgr.MaterialSystem = this;
		ShaderSystem.Services = services;
		ShaderSystem.MaterialSystem = this;
		ShaderSystem.ShaderAPI = ShaderAPI;
		ShaderSystem.Config = Config;
		ShaderShadow.HardwareConfig = HardwareConfig;
		MeshMgr.ShaderAPI = ShaderAPI;
		ShaderAPI.ShaderUtil = this;
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

	public bool SetMode(nint window, MaterialSystem_Config config) {
		int width = config.VideoMode.Width;
		int height = config.VideoMode.Height;
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

	public bool InFlashlightMode() {
		return GetRenderContextInternal().InFlashlightMode();
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
