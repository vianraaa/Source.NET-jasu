using OpenGL;

using Raylib_cs;

using Source.Common.MaterialSystem;
using Source.Common.ShaderAPI;
using Source.Common.ShaderLib;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Source.MaterialSystem;

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

public class ShaderAPIGl46 : IShaderAPI
{
	public TransitionTable TransitionTable;
	public StateSnapshot_t CurrentSnapshot;
	public MeshMgr MeshMgr;

	DynamicState DynamicState;

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
		throw new NotImplementedException();
	}

	public bool InFlashlightMode() {
		throw new NotImplementedException();
	}

	public void SetPixelShaderConstant(int v1, Span<float> flConsts, int v2) {
		throw new NotImplementedException();
	}

	public void SetVertexShaderIndex(int value) {
		throw new NotImplementedException();
	}

	MeshBase? RenderMesh;
	IMaterialInternal? Material;

	internal void RenderPass(byte pass, int passCount) {
		if (IsDeactivated())
			return;

		TransitionTable.UseSnapshot(CurrentSnapshot);
		if (RenderMesh != null)
			RenderMesh.RenderPass();
		else
			MeshMgr.RenderPassWithVertexAndIndexBuffers();
	}

	private bool IsDeactivated() {
		return false;
	}

	internal void InvalidateDelayedShaderConstraints() {
		throw new NotImplementedException();
	}

	public enum TransformType
	{
		IsIdentity = 0,
		IsCameraToWorld,
		IsGeneral
	}

	internal void BeginPass(StateSnapshot_t v) {
		throw new NotImplementedException();
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
		Warning("todo");
	}

	internal void ShadeMode(ShadeMode shadeMode) {
		if (DynamicState.ShadeMode != shadeMode) {
			DynamicState.ShadeMode = shadeMode;
		}
	}
}