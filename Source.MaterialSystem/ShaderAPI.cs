using Microsoft.Extensions.DependencyInjection;

using OpenGL;

using Raylib_cs;

using Source.Bitmap;
using Source.Common;
using Source.Common.Bitmap;
using Source.Common.Engine;
using Source.Common.Launcher;
using Source.Common.MaterialSystem;
using Source.Common.ShaderAPI;
using Source.Common.ShaderLib;

using Steamworks;

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using static Source.Common.Engine.IEngine;
using static Source.MaterialSystem.ShaderAPIGl46;

namespace Source.MaterialSystem;

public struct GfxViewport
{
	public int X;
	public int Y;
	public int Width;
	public int Height;
	public float MinZ;
	public float MaxZ;
}

public enum CommitFuncType
{
	PerDraw,
	PerPass
}

public class ShaderAPIGl46 : IShaderAPI, IShaderDevice
{
	public MeshMgr MeshMgr;
	[Imported] public IShaderSystem ShaderManager;

	public GraphicsDriver GetDriver() => Driver;

	uint glPipeline;

	public bool OnDeviceInit() {
		AcquireInternalRenderTargets();

		CreateMatrixStacks();

		ShaderManager.Init();
		MeshMgr.Init();

		InitRenderState();

		ClearColor4ub(0, 0, 0, 1);
		ClearBuffers(true, true, true, -1, -1);

		CreateShaderPipeline();

		return true;
	}

	private unsafe void CreateShaderPipeline() {
		fixed (uint* pipelinePtr = &glPipeline)
			glGenProgramPipelines(1, pipelinePtr);
		glBindProgramPipeline(glPipeline);
	}

	public void ClearBuffers(bool clearColor, bool clearDepth, bool clearStencil, int renderTargetWidth = -1, int renderTargetHeight = -1) {
		FlushBufferedPrimitives();
		uint flags = 0;

		if (clearColor)
			flags |= Gl46.GL_COLOR_BUFFER_BIT;
		if (clearDepth)
			flags |= Gl46.GL_DEPTH_BUFFER_BIT;
		if (clearStencil)
			flags |= Gl46.GL_STENCIL_BUFFER_BIT;

		Gl46.glClear(flags);
	}

	public void ClearColor3ub(byte r, byte g, byte b) => Gl46.glClearColor(r / 255f, g / 255f, b / 255f, 1);
	public void ClearColor4ub(byte r, byte g, byte b, byte a) => Gl46.glClearColor(r / 255f, g / 255f, b / 255f, a / 255f);

	public void BindVertexShader(in VertexShaderHandle vertexShader) {
		if (!vertexShader.IsValid()) {
			Warning("WARNING: Attempted to bind an invalid vertex shader!\n");
			return;
		}
		glUseProgramStages(glPipeline, GL_VERTEX_SHADER_BIT, (uint)vertexShader.Handle);
	}

	public void BindPixelShader(in PixelShaderHandle pixelShader) {
		if (!pixelShader.IsValid()) {
			Warning("WARNING: Attempted to bind an invalid pixel shader!\n");
			return;
		}
		glUseProgramStages(glPipeline, GL_FRAGMENT_SHADER_BIT, (uint)pixelShader.Handle);
	}

	public void CallCommitFuncs(CommitFuncType func, bool usingFixedFunction, bool force = false) {

	}

	private void CreateMatrixStacks() {

	}

	private void AcquireInternalRenderTargets() {

	}

	public void InitRenderState() {
		if (!IsDeactivated())
			ResetRenderState();
	}

	public void SetPresentParameters(in ShaderDeviceInfo info) {
		PresentParameters = info;
	}

	private void ResetRenderState(bool fullReset = true) {
		if (fullReset) {
			InitVertexAndPixelShaders();
		}

		SetDefaultState();
	}

	private void InitVertexAndPixelShaders() {
		// TODO; everything before this call
		ShaderManager.ResetShaderState();
	}

	public MaterialFogMode GetSceneFogMode() {
		return SceneFogMode;
	}

	MaterialFogMode SceneFogMode = MaterialFogMode.None;

	internal IShaderUtil ShaderUtil;

	public bool InFlashlightMode() {
		return ShaderUtil.InFlashlightMode();
	}

	public void SetPixelShaderConstant(int v1, Span<float> flConsts, int v2) {
		// todo
	}

	public void SetVertexShaderIndex(int value) {
		// ShaderManager()->SetVertexShaderIndex( vshIndex );
	}
	public void SetPixelShaderIndex(int value) {
		// ShaderManager()->SetPixelShaderIndex( vshIndex );
	}

	Mesh? RenderMesh;
	IMaterialInternal? Material;

	internal void RenderPass() {
		if (IsDeactivated())
			return;

		if (RenderMesh != null)
			RenderMesh.RenderPass();
		else
			MeshMgr.RenderPassWithVertexAndIndexBuffers();
	}

	private bool IsDeactivated() {
		return false;
	}

	internal void InvalidateDelayedShaderConstraints() {
		// TODO FIXME
	}

	public enum TransformType
	{
		IsIdentity = 0,
		IsCameraToWorld,
		IsGeneral
	}


	public void PushMatrix() {
		if (MatrixIsChanging()) {

		}
	}

	private bool MatrixIsChanging(TransformType type = TransformType.IsGeneral) {
		if (IsDeactivated())
			return false;

		if (type != TransformType.IsGeneral)
			return false;

		FlushBufferedPrimitivesInternal();

		return true;
	}
	public void FlushBufferedPrimitives() => FlushBufferedPrimitivesInternal();
	private void FlushBufferedPrimitivesInternal() {
		Assert(RenderMesh == null);
		MeshMgr.Flush();
	}

	public void PopMatrix() {
		if (MatrixIsChanging()) {
			UpdateMatrixTransform();
		}
	}

	private void UpdateMatrixTransform() {

	}

	public void DrawMesh(IMesh imesh) {
		Mesh mesh = (Mesh)imesh!;
		RenderMesh = mesh;
		VertexFormat vertexFormat = RenderMesh.GetVertexFormat();
		SetVertexDecl(vertexFormat, RenderMesh.HasColorMesh(), RenderMesh.HasFlexMesh(), Material!.IsUsingVertexID());
		CommitStateChanges();
		Material!.DrawMesh(VertexCompressionType.None);
		RenderMesh = null;
	}

	private void CommitStateChanges() {
		// todo
	}

	private void SetVertexDecl(VertexFormat vertexFormat, bool hasColorMesh, bool hasFleshMesh, bool usingMorph) {
		// Gl46.glVertexAttribPointer() i think we need here
	}

	public IMesh GetDynamicMesh(IMaterial material, int hwSkinBoneCount, bool buffered, IMesh? vertexOverride, IMesh? indexOverride) {
		Assert(material == null || material.IsRealTimeVersion());
		return MeshMgr.GetDynamicMesh(material, 0, hwSkinBoneCount, buffered, vertexOverride, indexOverride);
	}

	public void Bind(IMaterial? material) {
		IMaterialInternal? matInt = (IMaterialInternal?)material;

		bool materialChanged;
		if (Material != null && matInt != null && Material.InMaterialPage() && matInt.InMaterialPage()) {
			materialChanged = (Material.GetMaterialPage() != matInt.GetMaterialPage());
		}
		else {
			materialChanged = (Material != matInt) || (Material != null && Material.InMaterialPage()) || (matInt != null && matInt.InMaterialPage());
		}

		if (materialChanged) {
			FlushBufferedPrimitives();
			Material = matInt;
		}
	}

	internal void SetSkinningMatrices() {
		throw new NotImplementedException();
	}

	public void SetDefaultState() {
	}

	internal void ShadeMode(ShadeMode shadeMode) {
		throw new NotImplementedException();
	}

	public bool InEditorMode() {
		return false; // todo...?
	}

	public void SetVertexShaderConstant(int var, Span<float> vec) {
		SetVertexShaderConstantInternal(var, vec);
	}

	private void SetVertexShaderConstantInternal(int var, Span<float> vec) {
		// I'm so tired of looking at this stuff
	}

	bool UsingTextureRenderTarget;

	public void SetViewports(ReadOnlySpan<ShaderViewport> viewports) {
		Assert(viewports.Length == 1);
		if (viewports.Length != 1)
			return;

		GfxViewport viewport = new();
		viewport.X = viewports[0].TopLeftX;
		viewport.Y = viewports[0].TopLeftY;
		viewport.Width = viewports[0].Width;
		viewport.Height = viewports[0].Height;
		viewport.MinZ = viewports[0].MinZ;
		viewport.MaxZ = viewports[0].MaxZ;

		if (UsingTextureRenderTarget) {
			int maxWidth = 0, maxHeight = 0;
			GetBackBufferDimensions(out maxWidth, out maxHeight);
		}
	}

	public void GetViewports(Span<ShaderViewport> viewports) {
		throw new NotImplementedException();
	}

	public void GetBackBufferDimensions(out int width, out int height) {
		width = PresentParameters.DisplayMode.Width;
		height = PresentParameters.DisplayMode.Height;
	}

	ShaderDeviceInfo PresentParameters;
	bool ResetRenderStateNeeded = false;
	ulong CurrentFrame;
	nint TextureMemoryUsedLastFrame;

	public void BeginFrame() {
		if (ResetRenderStateNeeded) {
			ResetRenderState(false);
			ResetRenderStateNeeded = false;
		}

		++CurrentFrame;
		TextureMemoryUsedLastFrame = 0;
	}

	public void EndFrame() {
		ExportTextureList();
	}

	private void ExportTextureList() {

	}

	internal bool SetMode(nint window, in ShaderDeviceInfo info) {
		ShaderDeviceInfo actualInfo = info;
		if (!InitDevice(window, in actualInfo)) {
			return false;
		}

		if (!OnDeviceInit())
			return false;

		return true;
	}

	internal IServiceProvider services;
	internal IGraphicsContext? Device;
	internal GraphicsDriver Driver;

	public bool InitDevice(nint window, in ShaderDeviceInfo deviceInfo) {
		IGraphicsProvider graphics = services.GetRequiredService<IGraphicsProvider>();
		Device = graphics.CreateContext(in deviceInfo, window);
		if (Device == null)
			return false;

		Driver = deviceInfo.Driver;
		unsafe {
			if (deviceInfo.Driver.HasFlag(GraphicsDriver.OpenGL))
				GL_LoadExtensions(graphics.GL_LoadExtensionsPtr());
		}

		return true;
	}

	internal unsafe void GL_LoadExtensions(delegate* unmanaged[Cdecl]<byte*, void*> loadExts) {
		Gl46.Import((name) => (nint)loadExts((byte*)new Utf8Buffer(name).AsPointer()));
	}

	public bool IsActive() => Device != null;
	public bool IsUsingGraphics() => IsActive();


	public void Present() {
		FlushBufferedPrimitives();
		bool validPresent = true;
		bool inMainThread = ThreadInMainThread();

		if (!inMainThread) {
			throw new Exception();
		}

		glFlush();
	}

	bool IShaderDevice.IsDeactivated() => IsDeactivated();


	public void MatrixMode(MaterialMatrixMode i) {

	}

	public void LoadMatrix(in Matrix4x4 transposeTop) {

	}

	public int GetMatrixStack(MaterialMatrixMode mode) {
		Assert(mode >= 0 && mode < MaterialMatrixMode.Count);
		return (int)mode;
	}

	public void LoadIdentity() {

	}

	public int GetCurrentNumBones() {
		return 0;
	}

	public unsafe int LocateVertexShaderUniform(in VertexShaderHandle vertexShader, ReadOnlySpan<char> name) {
		if (!vertexShader.IsValid()) {
			Warning("WARNING: Attempted to locate uniform on an invalid vertex shader!\n");
			return -1;
		}
		Span<byte> bytes = stackalloc byte[name.Length * 2];
		int byteLen = Encoding.ASCII.GetBytes(name, bytes);
		int loc;
		fixed (byte* uniformName = bytes)
			loc = glGetUniformLocation((uint)vertexShader.Handle, uniformName);
		return loc;
	}

	public unsafe int LocatePixelShaderUniform(in PixelShaderHandle pixelShader, ReadOnlySpan<char> name) {
		if (!pixelShader.IsValid()) {
			Warning("WARNING: Attempted to locate uniform on an invalid pixel shader!\n");
			return -1;
		}
		Span<byte> bytes = stackalloc byte[name.Length * 2];
		int byteLen = Encoding.ASCII.GetBytes(name, bytes);
		int loc;
		fixed (byte* uniformName = bytes)
			loc = glGetUniformLocation((uint)pixelShader.Handle, uniformName);
		return loc;
	}

	public void SetVertexShaderUniform(in VertexShaderHandle vertexShader, int uniform, int integer) {
		throw new NotImplementedException();
	}

	public void SetVertexShaderUniform(in VertexShaderHandle vertexShader, int uniform, float fl) {
		throw new NotImplementedException();
	}

	public void SetVertexShaderUniform(in VertexShaderHandle vertexShader, int uniform, ReadOnlySpan<float> flConsts) {
		throw new NotImplementedException();
	}

	public void SetPixelShaderUniform(in PixelShaderHandle pixelShader, int uniform, int integer) {
		throw new NotImplementedException();
	}

	public void SetPixelShaderUniform(in PixelShaderHandle pixelShader, int uniform, float fl) {
		throw new NotImplementedException();
	}

	public void SetPixelShaderUniform(in PixelShaderHandle pixelShader, int uniform, ReadOnlySpan<float> flConsts) {
		throw new NotImplementedException();
	}

	internal void BindTexture(in MaterialVarGPU hardwareTarget, int frame, ShaderAPITextureHandle_t v) {
		throw new NotImplementedException();
	}

	public bool CanDownloadTextures() {
		if (IsDeactivated())
			return false;

		return IsActive();
	}

	ShaderAPITextureHandle_t textureModifyTarget;

	public void ModifyTexture(ShaderAPITextureHandle_t textureHandle) {
		textureModifyTarget = textureHandle;
	}

	public struct TextureLoadInfo {
		public ShaderAPITextureHandle_t Handle;
		public int Width;
		public int Height;
		public int ZOffset;
		public int Level;
		public ImageFormat SrcFormat;
	}

	public void TexImageFromVTF(IVTFTexture? vtf, int vtfFrame) {
		Assert(vtf != null);
		Assert(textureModifyTarget != INVALID_SHADERAPI_TEXTURE_HANDLE);

		ref TextureLoadInfo info = ref (stackalloc TextureLoadInfo[1])[0];
		info.Handle = textureModifyTarget;
		info.Width = 0;
		info.Height = 0;
		info.ZOffset = 0;
		info.Level = 0;
		info.SrcFormat = vtf.Format();

		if (vtf.Depth() > 1) {
			throw new NotImplementedException("Multidepth textures not supported yet");
		}
		else if (vtf.IsCubeMap()) {
			throw new NotImplementedException("Cubemap textures not supported yet");
		}
		else {
			LoadTextureFromVTF(in info, vtf, vtfFrame);
		}
	}

	private unsafe void LoadTextureFromVTF(in TextureLoadInfo info, IVTFTexture vtf, int vtfFrame) {
		vtf.ImageFileInfo(vtfFrame, 0, info.Level, out int start, out int size);

		if (info.SrcFormat.IsCompressed()) {
			Span<byte> data = vtf.ImageData(vtfFrame, 0, info.Level);
			fixed (byte* bytes = data)
			glCompressedTextureSubImage2D((uint)info.Handle, info.Level, 0, 0, vtf.Width(), vtf.Height(), ImageLoader.GetGLImageFormat(info.SrcFormat), data.Length, bytes);
		}
		else {
			Span<byte> data = vtf.ImageData(vtfFrame, 0, info.Level);
			fixed (byte* bytes = data)
				glTextureSubImage2D((uint)info.Handle, info.Level, 0, 0, vtf.Width(), vtf.Height(), ImageLoader.GetGLImageFormat(info.SrcFormat), data.Length, bytes);
		}
	}

	public unsafe void CreateTextures(
		ShaderAPITextureHandle_t[] textureHandles,
		int count,
		int width,
		int height,
		int depth,
		ImageFormat imageFormat,
		ushort mipCount,
		int copies,
		CreateTextureFlags creationFlags,
		ReadOnlySpan<char> debugName,
		ReadOnlySpan<char> textureGroup) {
		if (depth == 0)
			depth = 1;

		bool isCubeMap = (creationFlags & CreateTextureFlags.Cubemap) != 0;
		bool isRenderTarget = (creationFlags & CreateTextureFlags.RenderTarget) != 0;
		bool managed = (creationFlags & CreateTextureFlags.Managed) != 0;
		bool isDepthBuffer = (creationFlags & CreateTextureFlags.DepthBuffer) != 0;
		bool isDynamic = (creationFlags & CreateTextureFlags.Dynamic) != 0;
		bool isSRGB = (creationFlags & CreateTextureFlags.SRGB) != 0;

		fixed (ShaderAPITextureHandle_t* handles = textureHandles)
			glCreateTextures(GL_TEXTURE_2D, textureHandles.Length, (uint*)handles);

		for (int i = 0; i < count; i++) {
			ShaderAPITextureHandle_t handle = textureHandles[i];
			glTextureStorage2D((uint)handle, mipCount, ImageLoader.GetGLImageFormat(imageFormat), width, height);
		}
	}
	public ShaderAPITextureHandle_t CreateDepthTexture(ImageFormat imageFormat, ushort width, ushort height, Span<char> debugName, bool v) {
		throw new NotImplementedException();
	}

	internal bool IsTexture(ShaderAPITextureHandle_t handle) {
		return true; // TODO
	}

	internal void DeleteTexture(ShaderAPITextureHandle_t handle) {
		// TODO
	}

	public ImageFormat GetNearestSupportedFormat(ImageFormat fmt, bool filteringRequired = true) {
		return FindNearestSupportedFormat(fmt, false, false, filteringRequired);
	}

	public ImageFormat FindNearestSupportedFormat(ImageFormat format, bool isVertexTexture, bool isRenderTarget, bool filterableRequired) {
		return format;
	}

	public int GetCurrentDynamicVBSize() {
		return (1024 + 512) * 1024; // See if it's still needed to use smaller sizes at certain points... how would this even work, I wonder
	}
}