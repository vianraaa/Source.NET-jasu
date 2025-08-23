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

public struct GfxViewport {
	public int X;
	public int Y;
	public int Width;
	public int Height;
	public float MinZ;
	public float MaxZ;
}

public struct TextureStageShadowState
{
	public uint ColorOp;
	public int ColorArg1;
	public int ColorArg2;
	public uint AlphaOp;
	public int AlphaArg1;
	public int AlphaArg2;
	public int TexCoordIndex;

	public const int SIZEOF = 4 * 7;
}
public struct SamplerShadowState
{
	public bool TextureEnable;
	public bool SRGBReadEnable;
	public bool Fetch4Enable;
	public bool ShadowFilterEnable;
	public const byte SIZEOF = 4;
}

public unsafe struct ShadowState
{
	public const int MAX_SAMPLERS = 16;
	public const int MAX_TEXTURE_STAGES = 16;

	public uint ZFunc;
	public uint ZEnable;
	public uint ColorWriteEnable;
	public uint FillMode;
	public uint SrcBlend;
	public uint DestBlend;
	public uint BlendOp;
	public uint SrcBlendAlpha;
	public uint DestBlendAlpha;
	public uint BlendOpAlpha;
	public uint AlphaFunc;
	public uint AlphaRef;
	// Wow! That's bad!
	// But I don't think there's another "good way" to do it because the constructor
	// won't get called...
	fixed byte __textureStage[MAX_TEXTURE_STAGES * TextureStageShadowState.SIZEOF];
	fixed byte __samplerState[MAX_SAMPLERS * SamplerShadowState.SIZEOF];

	public Span<TextureStageShadowState> TextureStage {
		get {
			fixed (byte* bPtr = __textureStage)
				return new(bPtr, MAX_TEXTURE_STAGES);
		}
	}
	public Span<SamplerShadowState> SamplerState {
		get {
			fixed (byte* bPtr = __samplerState)
				return new(bPtr, MAX_SAMPLERS);
		}
	}

	public ShaderFogMode FogMode;
	public bool ZWriteEnable;
	public byte ZBias;
	public bool CullEnable;
	public bool Lighting;
	public bool SpecularEnable;
	public bool AlphaBlendEnable;
	public bool AlphaTestEnable;
	public bool UsingFixedFunction;
	public bool VertexBlendEnable;
	public bool SRGBWriteEnable;
	public bool SeparateAlphaBlendEnable;
	public bool StencilEnable;
	public bool DisableFogGammaCorrection;
	public bool EnableAlphaToCoverage;
}


public struct CurrentTextureStageState
{
	public uint ColorOp;
	public int ColorArg1;
	public int ColorArg2;
	public uint AlphaOp;
	public int AlphaArg1;
	public int AlphaArg2;

	public const int SIZEOF = 6 * 4;
};
public struct CurrentSamplerState
{
	public bool SRGBReadEnable;
	public bool Fetch4Enable;
	public bool ShadowFilterEnable;

	public const int SIZEOF = 3;
};
public unsafe struct CurrentState
{
	public bool AlphaBlendEnable;
	public uint SrcBlend;
	public uint DestBlend;
	public uint BlendOp;

	public bool SeparateAlphaBlendEnable;
	public uint SrcBlendAlpha;
	public uint DestBlendAlpha;
	public uint BlendOpAlpha;

	public bool ZEnable;
	public uint ZFunc;
	public PolygonOffsetMode ZBias;

	public bool AlphaTestEnable;
	public uint AlphaFunc;
	public uint AlphaRef;

	public bool ForceDepthFuncEquals;
	public bool OverrideDepthEnable;
	public bool OverrideZWriteEnable;

	public bool OverrideAlphaWriteEnable;
	public bool OverriddenAlphaWriteValue;
	public bool OverrideColorWriteEnable;
	public bool OverriddenColorWriteValue;
	public uint m_ColorWriteEnable;

	public bool OverrideBlendEnable;
	public bool OverriddenBlendWriteValue;
	public bool OverrideBlendSeperateAlphaEnable;
	public bool OverriddenBlendSeperateAlphaWriteValue;

	public bool LinearColorSpaceFrameBufferEnable;

	public bool StencilEnable;
	public uint StencilFunc;
	public int StencilRef;
	public int StencilMask;
	public uint StencilFail;
	public uint StencilZFail;
	public uint StencilPass;
	public int StencilWriteMask;

	fixed byte __textureStage[ShadowState.MAX_TEXTURE_STAGES * CurrentTextureStageState.SIZEOF];
	fixed byte __samplerState[ShadowState.MAX_SAMPLERS * CurrentSamplerState.SIZEOF];

	public Span<CurrentTextureStageState> TextureStage {
		get {
			fixed (byte* bPtr = __textureStage)
				return new(bPtr, ShadowState.MAX_TEXTURE_STAGES);
		}
	}

	public Span<CurrentSamplerState> SamplerState {
		get {
			fixed (byte* bPtr = __samplerState)
				return new(bPtr, ShadowState.MAX_SAMPLERS);
		}
	}
}



public struct ShadowShaderState
{
	public VertexShaderHandle VertexShader;
	public PixelShaderHandle PixelShader;

	public VertexFormat VertexUsage;
	public bool ModulateConstantColor;
}

public struct TextureStageState
{
	public int TexCoordIndex;
	public int TexCoordinate;
	public float OverbrightVal;
	public ShaderTexArg[][] Arg;
	public ShaderTexOp[] Op;
	public bool TexGenEnable;
	public bool TextureAlphaEnable;
}

public struct SamplerState
{
	public bool TextureEnable;
}

public unsafe struct DynamicState
{
	public int NumBones;
	internal ShadeMode ShadeMode;
}

public enum CommitFuncType
{
	PerDraw,
	PerPass
}

public class ShaderAPIGl46 : IShaderAPI, IShaderDevice
{
	public TransitionTable TransitionTable;
	public ShaderShadowGl46 ShaderShadow;
	public StateSnapshot_t CurrentSnapshot;
	public MeshMgr MeshMgr;
	[Imported] public IShaderSystem ShaderManager;

	DynamicState DynamicState;
	DynamicState DesiredState;

	public bool OnDeviceInit() {
		AcquireInternalRenderTargets();

		CreateMatrixStacks();

		ShaderManager.Init();
		ShaderShadow.Init();
		MeshMgr.Init();
		TransitionTable.Init();

		InitRenderState();

		ClearColor4ub(0, 0, 0, 1);
		ClearBuffers(true, true, true, -1, -1);

		return true;
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

	public void BindVertexShader(in VertexShaderHandle vertexShader) => throw new NotImplementedException();
	public void BindGeometryShader(in GeometryShaderHandle geometryShader) => throw new NotImplementedException();
	public void BindPixelShader(in PixelShaderHandle pixelShader) => throw new NotImplementedException();

	public void CallCommitFuncs(CommitFuncType func, bool usingFixedFunction, bool force = false) {

	}

	private void CreateMatrixStacks() {

	}

	private void AcquireInternalRenderTargets() {

	}

	public void InitRenderState() {
		ShaderShadow.SetDefaultState();
		TransitionTable.TakeDefaultStateSnapshot();
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

		TransitionTable.UseDefaultState();
		SetDefaultState();
	}

	private void InitVertexAndPixelShaders() {
		// TODO; everything before this call
		ShaderManager.ResetShaderState();
	}

	public VertexFormat ComputeVertexFormat(Span<StateSnapshot_t> snapshots) {
		return ComputeVertexUsage(snapshots);
	}

	public VertexFormat ComputeVertexUsage(Span<StateSnapshot_t> snapshots) {
		if (snapshots.Length == 0)
			return 0;

		if (snapshots.Length == 1) {
			ref ShadowShaderState state = ref TransitionTable.GetSnapshotShader(snapshots[0]);
			return state.VertexUsage;
		}

		VertexCompressionType compression = VertexCompressionType.None;
		int userDataSize = 0, numBones = 0, flags = 0;
		Span<int> texCoordSize = [0, 0, 0, 0, 0, 0, 0, 0];
		for (int i = snapshots.Length; --i >= 0;) {
			ref ShadowShaderState state = ref TransitionTable.GetSnapshotShader(snapshots[i]);
			VertexFormat fmt = state.VertexUsage;
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

	public bool IsAlphaTested(StateSnapshot_t id) {
		return TransitionTable.GetSnapshot(id).AlphaBlendEnable;
	}

	public bool IsTranslucent(StateSnapshot_t id) {
		return TransitionTable.GetSnapshot(id).AlphaTestEnable;
	}
	public bool IsDepthWriteEnabled(StateSnapshot_t id) {
		return TransitionTable.GetSnapshot(id).ZWriteEnable;
	}

	public bool UsesVertexAndPixelShaders(StateSnapshot_t id) {
		return TransitionTable.GetSnapshotShader(id).VertexShader != VertexShaderHandle.INVALID;
	}

	public StateSnapshot_t TakeSnapshot() {
		return TransitionTable.TakeSnapshot();
	}

	public int GetCurrentNumBones() {
		return DynamicState.NumBones;
	}

	public void SetNumBoneWeights(int bones) {
		if (DynamicState.NumBones != bones) {
			FlushBufferedPrimitives();
			DynamicState.NumBones = GetCurrentNumBones();
			if (!Unsafe.IsNullRef(ref TransitionTable.CurrentShadowState())) {
				SetVertexBlendState(TransitionTable.CurrentShadowState().VertexBlendEnable ? -1 : 0);
			}
		}
	}

	private void SetVertexBlendState(int numBones) {
		if (numBones < 0)
			numBones = DynamicState.NumBones;

		if (numBones > 0)
			--numBones;

		// TODO: rest of this 
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

	internal void RenderPass(byte pass, int passCount) {
		if (IsDeactivated())
			return;

		Assert(CurrentSnapshot != -1);
		TransitionTable.UseSnapshot(CurrentSnapshot);
		CommitPerPassStateChanges(CurrentSnapshot);
		if (RenderMesh != null)
			RenderMesh.RenderPass();
		else
			MeshMgr.RenderPassWithVertexAndIndexBuffers();
	}

	private void CommitPerPassStateChanges(short currentSnapshot) {

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

	internal void BeginPass(StateSnapshot_t snapshot) {
		CurrentSnapshot = snapshot;
		if (RenderMesh != null)
			RenderMesh.BeginPass();
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

	bool InSelectionMode;

	public bool IsInSelectionMode() {
		return InSelectionMode;
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
		if (DynamicState.ShadeMode != shadeMode) {
			DynamicState.ShadeMode = shadeMode;
		}
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

	public bool InitDevice(nint window, in ShaderDeviceInfo deviceInfo) {
		IGraphicsProvider graphics = services.GetRequiredService<IGraphicsProvider>();
		Device = graphics.CreateContext(in deviceInfo, window);
		if (Device == null)
			return false;

		unsafe {
			if (deviceInfo.Driver.HasFlag(GraphicsAPIVersion.OpenGL))
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

	internal void ApplyZBias(in ShadowState state) {

	}

	bool IShaderDevice.IsDeactivated() => IsDeactivated();

	internal void ApplyTextureEnable(in ShadowState state, int i) {
		Warning("WARNING: Tried to call ShaderAPIGl46.ApplyTextureEnable, not implemented!!!\n");
	}

	internal void ApplyAlphaToCoverage(bool enableAlphaToCoverage) {
		Warning("WARNING: Tried to call ShaderAPIGl46.ApplyAlphaToCoverage, not implemented!!!\n");
	}

	internal void ApplyCullEnable(bool cullEnable) {
		Warning("WARNING: Tried to call ShaderAPIGl46.ApplyCullEnable, not implemented!!!\n");
	}

	internal void ApplyVertexBlendEnable(bool vertexBlendEnable) {
		Warning("WARNING: Tried to call ShaderAPIGl46.ApplyVertexBlendEnable, not implemented!!!\n");
	}

	internal void ApplyFogMode(ShaderFogMode fogMode) {
		Warning("WARNING: Tried to call ShaderAPIGl46.ApplyFogMode, not implemented!!!\n");
	}
}