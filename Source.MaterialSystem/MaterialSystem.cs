using CommunityToolkit.HighPerformance;

using Microsoft.Extensions.DependencyInjection;

using Raylib_cs;

using Source.Common;
using Source.Common.Commands;
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
		services.AddSingleton<ShaderAPIGl46>();
		services.AddSingleton<IShaderAPI>(x => x.GetRequiredService<ShaderAPIGl46>());
		services.AddSingleton<IShaderDevice>(x => x.GetRequiredService<ShaderAPIGl46>());
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
	public readonly IShaderDevice ShaderDevice;
	public readonly ShaderAPIGl46 ShaderAPI;
	public readonly ShaderShadowGl46 ShaderShadow;
	public readonly MeshMgr MeshMgr;
	public readonly HardwareConfig HardwareConfig;
	public readonly MaterialSystem_Config Config;

	public MaterialSystem(IServiceProvider services) {
		this.services = services;

		FileSystem = services.GetRequiredService<IFileSystem>();
		ShaderAPI = (services.GetRequiredService<IShaderAPI>() as ShaderAPIGl46)!;
		ShaderDevice = services.GetRequiredService<IShaderDevice>();
		ShaderShadow = (services.GetRequiredService<IShaderShadow>() as ShaderShadowGl46)!;
		TextureSystem = (services.GetRequiredService<ITextureManager>() as TextureManager)!;
		MeshMgr = (services.GetRequiredService<MeshMgr>() as MeshMgr)!; // todo: interface
		HardwareConfig = (services.GetRequiredService<IMaterialSystemHardwareConfig>() as HardwareConfig)!; // todo: interface
		ShaderSystem = (services.GetRequiredService<IShaderSystem>() as ShaderSystem)!;
		Config = services.GetRequiredService<MaterialSystem_Config>()!;

		// Link up
		MeshMgr.MaterialSystem = this;
		MeshMgr.ShaderAPI = ShaderAPI;

		ShaderAPI.MeshMgr = MeshMgr;
		ShaderAPI.ShaderManager = ShaderSystem;
		ShaderAPI.TransitionTable = new(ShaderShadow);
		ShaderAPI.TransitionTable.HardwareConfig = HardwareConfig;
		ShaderAPI.TransitionTable.ShaderAPI = ShaderAPI;
		ShaderAPI.TransitionTable.ShaderDevice = ShaderDevice;
		ShaderAPI.TransitionTable.ShaderManager = ShaderSystem;

		ShaderAPI.services = services;

		TextureSystem.MaterialSystem = this;

		ShaderAPI.ShaderShadow = ShaderShadow;
		ShaderAPI.ShaderUtil = this;

		ShaderShadow.HardwareConfig = HardwareConfig;
		ShaderShadow.MeshMgr = MeshMgr;

		ShaderSystem.Config = Config;
		ShaderSystem.MaterialSystem = this;
		ShaderSystem.Services = services;
		ShaderSystem.ShaderAPI = ShaderAPI;

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

	public static ConVar mat_fullbright = new("0", FCvar.Cheat);
	public static ConVar mat_normalmaps = new("0", FCvar.Cheat);


	public static ConVar mat_specular = new("1", 0, "Enable/Disable specularity for perf testing.  Will cause a material reload upon change.");
	public static ConVar mat_bumpmap = new("1", 0);
	public static ConVar mat_phong = new("1", 0);
	public static ConVar mat_parallaxmap = new("1", FCvar.Hidden);
	public static ConVar mat_reducefillrate = new("0", 0);

	public uint DebugVarsSignature;

	public unsafe void BeginFrame(double frameTime) {
		if (!ThreadInMainThread() || IsInFrame())
			return;


		DebugVarsSignature = (uint)(
			((mat_specular.GetInt() != 0) ? 1 : 0)
			+ (mat_normalmaps.GetInt() << 1)
			+ (mat_fullbright.GetInt() << 2)
		);

		var renderContext = GetRenderContextInternal();

		renderContext.MarkRenderDataUnused(true);
		renderContext.BeginFrame();
		renderContext.SetFrameTime(frameTime);

		Assert(!InFrame);
		InFrame = true;
	}

	bool InFrame = false;


	private Matrix4x4 GetScreenMatrix() {
		launcherMgr.DisplayedSize(out int screenWidth, out _);
		int renderWidth = 0, renderHeight = 0;
		launcherMgr.RenderedSize(false, ref renderWidth, ref renderHeight);
		float scaleRatio = (float)renderWidth / (float)screenWidth;
		return Matrix4x4.CreateScale(scaleRatio, scaleRatio, 1);
	}

	public bool IsInFrame() => InFrame;

	public void EndFrame() {
		if (!ThreadInMainThread() || !IsInFrame())
			return;

		GetRenderContextInternal().EndFrame();

		Assert(InFrame);
		InFrame = false;
	}

	ulong FrameNum;

	public void SwapBuffers() {
		GetRenderContextInternal().SwapBuffers();
		FrameNum++;
	}

	ThreadLocal<MatRenderContext> matContext;
	public IMatRenderContext GetRenderContext() => matContext!.Value!;

	public bool SetMode(nint window, MaterialSystem_Config config) {
		int width = config.VideoMode.Width;
		int height = config.VideoMode.Height;

		bool previouslyUsingGraphics = ShaderDevice.IsUsingGraphics();
		ConvertModeStruct(config, out ShaderDeviceInfo info);
		if (!ShaderAPI.SetMode(window, in info))
			return false;

		launcherMgr.RenderedSize(true, ref width, ref height);
		return true;
	}

	private void ConvertModeStruct(MaterialSystem_Config config, out ShaderDeviceInfo mode) {
		mode = new ShaderDeviceInfo();
		mode.DisplayMode.Width = config.VideoMode.Width;
		mode.DisplayMode.Height = config.VideoMode.Height;
		mode.DisplayMode.Format = config.VideoMode.Format;
		mode.DisplayMode.RefreshRateNumerator = config.VideoMode.RefreshRate;
		mode.DisplayMode.RefreshRateDenominator = config.VideoMode.RefreshRate >= 0 ? 1 : 0;
		mode.BackBufferCount = 1;
		mode.AASamples = config.AASamples;
		mode.AAQuality = config.AAQuality;
		mode.Driver = config.Driver;
		mode.WindowedSizeLimitWidth = (int)config.WindowedSizeLimitWidth;
		mode.WindowedSizeLimitHeight = (int)config.WindowedSizeLimitHeight;

		mode.Windowed = config.Windowed();
		mode.Resizing = config.Resizing();
		mode.UseStencil = config.Stencil();
		mode.LimitWindowedSize = config.LimitWindowedSize();
		mode.WaitForVSync = config.WaitForVSync();
		mode.ScaleToOutputResolution = config.ScaleToOutputResolution();
		mode.UsingMultipleWindows = config.UsingMultipleWindows();
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

	public bool OnSetPrimitiveType(IMesh mesh, MaterialPrimitiveType type) {
		throw new NotImplementedException();
	}

	public bool OnFlushBufferedPrimitives() {
		throw new NotImplementedException();
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
