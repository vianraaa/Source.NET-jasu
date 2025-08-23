using Microsoft.Extensions.DependencyInjection;

using OpenGL;

using Raylib_cs;

using Source.Common.Engine;
using Source.Common.Launcher;
using Source.Common.MaterialSystem;
using Source.Common.ShaderAPI;
using Source.Common.ShaderLib;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using static Source.Common.Engine.IEngine;

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

	public VertexFormat ComputeVertexFormat(Span<VertexFormat> formats) {
		return ComputeVertexUsage(formats);
	}

	public VertexFormat ComputeVertexUsage(Span<VertexFormat> formats) {
		if (formats.Length == 0)
			return 0;

		if (formats.Length == 1) {
			return formats[0];
		}

		VertexCompressionType compression = VertexCompressionType.None;
		int userDataSize = 0, numBones = 0, flags = 0;
		Span<int> texCoordSize = [0, 0, 0, 0, 0, 0, 0, 0];
		for (int i = formats.Length; --i >= 0;) {
			VertexFormat fmt = formats[i];
			flags |= fmt.VertexFlags();

			VertexCompressionType newCompression = fmt.CompressionType();
			if (compression != newCompression && compression != VertexCompressionType.Invalid) {
				Warning("Encountered a material with two passes that specify different vertex compression types!\n");
				compression = VertexCompressionType.Invalid;
			}

			int newNumBones = fmt.NumBoneWeights();
			if ((numBones != newNumBones) && newNumBones != 0) {
				if (numBones != 0) {
					Warning("Encountered a material with two passes that use different numbers of bones!\n");
				}
				numBones = newNumBones;
			}

			int newUserSize = fmt.UserDataSize();
			if ((userDataSize != newUserSize) && (newUserSize != 0)) {
				if (userDataSize != 0) {
					Warning("Encountered a material with two passes that use different user data sizes!\n");
				}
				userDataSize = newUserSize;
			}

			for (int j = 0; j < IMesh.VERTEX_MAX_TEXTURE_COORDINATES; ++j) {
				int newSize = fmt.TexCoordSize(j);
				if ((texCoordSize[j] != newSize) && (newSize != 0)) {
					if (texCoordSize[j] != 0) {
						Warning("Encountered a material with two passes that use different texture coord sizes!\n");
					}
					if (texCoordSize[j] < newSize) {
						texCoordSize[j] = newSize;
					}
				}
			}
		}

		return MeshMgr.ComputeVertexFormat(flags, IMesh.VERTEX_MAX_TEXTURE_COORDINATES, texCoordSize, numBones, userDataSize);
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

	MeshBase? RenderMesh;
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
		MeshBase mesh = (MeshBase)imesh!;
		RenderMesh = mesh;
		VertexFormat vertexFormat = RenderMesh.GetVertexFormat();
		SetVertexDecl(vertexFormat, RenderMesh.HasColorMesh(), RenderMesh.HasFlexMesh(), Material!.IsUsingVertexID());
		CommitStateChanges();
		Material!.DrawMesh(vertexFormat.CompressionType());
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
}