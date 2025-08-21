using Source.Common.MaterialSystem;
using Source.Common.ShaderAPI;

using System;
using System.Collections.Generic;
using System.Linq;
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
}
public struct SamplerShadowState
{
	public bool TextureEnable;
	public bool SRGBReadEnable;
	public bool Fetch4Enable;
	public bool ShadowFilterEnable;
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
	public TextureStageShadowState[] TextureStage;
	public SamplerShadowState[] SamplerState;
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

	public ShadowState() {
		TextureStage = new TextureStageShadowState[MAX_TEXTURE_STAGES];
		SamplerState = new SamplerShadowState[MAX_SAMPLERS];
	}
}

public struct ShadowShaderState
{
	public VertexShaderHandle VertexShader;
	public PixelShaderHandle PixelShader;

	public VertexFormat VertexUsage;
	public bool ModulateConstantColor;
}

public class ShaderShadowGl46 : IShaderShadow
{
	ShadowState ShadowState;
	ShadowShaderState ShadowShaderState;
	public HardwareConfig HardwareConfig;
	public ref ShadowState GetShadowState() {
		return ref ShadowState;
	}
	public ref ShadowShaderState GetShadowShaderState() {
		return ref ShadowShaderState;
	}
	public void ComputeAggregateShadowState() {

	}

	public void DepthFunc(ShaderDepthFunc nearer) {

	}

	public void EnableAlphaToCoverage(bool v) {

	}

	public void EnableCulling(bool v) {

	}

	public void EnableDepthTest(bool v) {

	}

	public void EnableDepthWrites(bool v) {

	}

	public void EnablePolyOffset(PolygonOffsetMode decal) {

	}

	public void PolyMode(ShaderPolyModeFace frontAndBack, ShaderPolyMode line) {

	}

	public void SetDefaultState() {
		DepthFunc(ShaderDepthFunc.NearerOrEqual);
		EnableDepthWrites(true);
		EnableDepthTest(true);
		EnableColorWrites(true);
		EnableAlphaWrites(false);
		EnableAlphaTest(false);
		EnableLighting(false);
		EnableConstantColor(false);
		EnableBlending(false);
		BlendFunc(ShaderBlendFactor.One, ShaderBlendFactor.Zero);
		BlendOp(ShaderBlendOp.Add);
		// GR - separate alpha
		EnableBlendingSeparateAlpha(false);
		BlendFuncSeparateAlpha(ShaderBlendFactor.One, ShaderBlendFactor.Zero);
		BlendOpSeparateAlpha(ShaderBlendOp.Add);
		AlphaFunc(ShaderAlphaFunc.GreaterEqual, 0.7f);
		PolyMode(ShaderPolyModeFace.FrontAndBack, ShaderPolyMode.Fill);
		EnableCulling(true);
		EnableAlphaToCoverage(false);
		EnablePolyOffset(PolygonOffsetMode.Disable);
		EnableVertexBlend(false);
		EnableSpecular(false);
		EnableSRGBWrite(false);
		DrawFlags(ShaderDrawBitField.Position);
		EnableCustomPixelPipe(false);
		CustomTextureStages(0);
		EnableAlphaPipe(false);
		EnableConstantAlpha(false);
		EnableVertexAlpha(false);
		SetVertexShader(null, 0);
		SetPixelShader(null, 0);
		FogMode(ShaderFogMode.Disabled);
		DisableFogGammaCorrection(false);
		SetDiffuseMaterialSource(ShaderMaterialSource.Material);
		EnableStencil(false);
		StencilFunc(ShaderStencilFunc.Always);
		StencilPassOp(ShaderStencilOp.Keep);
		StencilFailOp(ShaderStencilOp.Keep);
		StencilDepthFailOp(ShaderStencilOp.Keep);
		StencilReference(0);
		StencilMask(unchecked((int)0xFFFFFFFF));
		StencilWriteMask(unchecked((int)0xFFFFFFFF));
		ShadowShaderState.VertexUsage = 0;

		int i;
		int nSamplerCount = HardwareConfig.GetSamplerCount();
		for (i = 0; i < nSamplerCount; i++) {
			EnableTexture((Sampler)i, false);
			EnableSRGBRead((Sampler)i, false);
		}

		int nTextureStageCount = HardwareConfig.GetTextureStageCount();
		for (i = 0; i < nTextureStageCount; i++) {
			EnableTexGen((TextureStage)i, false);
			OverbrightValue((TextureStage)i, 1.0f);
			EnableTextureAlpha((TextureStage)i, false);
			CustomTextureOperation((TextureStage)i, ShaderTexChannel.Color, ShaderTexOp.Disable, ShaderTexArg.Texture, ShaderTexArg.PreviousStage);
			CustomTextureOperation((TextureStage)i, ShaderTexChannel.Alpha, ShaderTexOp.Disable, ShaderTexArg.Texture, ShaderTexArg.PreviousStage);
		}
	}

	public void EnableStencil(bool enable) {

	}

	public void StencilFunc(ShaderStencilFunc stencilFunc) {

	}

	public void StencilPassOp(ShaderStencilOp stencilOp) {

	}

	public void StencilFailOp(ShaderStencilOp stencilOp) {

	}

	public void StencilDepthFailOp(ShaderStencilOp stencilOp) {

	}

	public void StencilReference(int nReference) {

	}

	public void StencilMask(int nMask) {

	}

	public void StencilWriteMask(int nMask) {

	}

	public void EnableColorWrites(bool enable) {

	}

	public void EnableAlphaWrites(bool enable) {

	}

	public void EnableBlending(bool enable) {

	}

	public void BlendFunc(ShaderBlendFactor srcFactor, ShaderBlendFactor dstFactor) {

	}

	public void EnableAlphaTest(bool enable) {

	}

	public void AlphaFunc(ShaderAlphaFunc alphaFunc, float alphaRef) {

	}

	public void EnableConstantColor(bool enable) {

	}

	public void VertexShaderVertexFormat(uint flags, int texCoordCount, Span<int> texCoordDimensions, int userDataSize) {

	}

	public void SetVertexShader(ReadOnlySpan<char> fileName, int nStaticVshIndex) {

	}

	public void SetPixelShader(ReadOnlySpan<char> fileName, int nStaticPshIndex = 0) {

	}

	public void EnableLighting(bool enable) {

	}

	public void EnableSpecular(bool enable) {

	}

	public void EnableSRGBWrite(bool enable) {

	}

	public void EnableSRGBRead(Sampler sampler, bool enable) {

	}

	public void EnableVertexBlend(bool enable) {

	}

	public void OverbrightValue(TextureStage stage, float value) {

	}

	public void EnableTexture(Sampler sampler, bool enable) {

	}

	public void EnableTexGen(TextureStage stage, bool enable) {

	}

	public void TexGen(TextureStage stage, ShaderTexGenParam param) {

	}

	public void EnableCustomPixelPipe(bool enable) {

	}

	public void CustomTextureStages(int stageCount) {

	}

	public void CustomTextureOperation(TextureStage stage, ShaderTexChannel channel, ShaderTexOp op, ShaderTexArg arg1, ShaderTexArg arg2) {

	}

	public void DrawFlags(ShaderDrawBitField drawFlags) {

	}

	public void EnableAlphaPipe(bool enable) {

	}

	public void EnableConstantAlpha(bool enable) {

	}

	public void EnableVertexAlpha(bool enable) {

	}

	public void EnableTextureAlpha(TextureStage stage, bool enable) {

	}

	public void EnableBlendingSeparateAlpha(bool enable) {

	}

	public void BlendFuncSeparateAlpha(ShaderBlendFactor srcFactor, ShaderBlendFactor dstFactor) {

	}

	public void FogMode(ShaderFogMode fogMode) {

	}

	public void SetDiffuseMaterialSource(ShaderMaterialSource materialSource) {

	}

	public void DisableFogGammaCorrection(bool bDisable) {

	}

	public void SetShadowDepthFiltering(Sampler stage) {

	}

	public void BlendOp(ShaderBlendOp blendOp) {

	}

	public void BlendOpSeparateAlpha(ShaderBlendOp blendOp) {

	}
}

public class ShaderAPIGl46 : IShaderAPI
{
	public TransitionTable TransitionTable;
	public StateSnapshot_t CurrentSnapshot;
	public MeshMgr MeshMgr;

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
		throw new NotImplementedException();
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

	internal void RenderPass(byte renderPass, int passCount) {
		throw new NotImplementedException();
	}

	internal void InvalidateDelayedShaderConstraints() {
		throw new NotImplementedException();
	}

	internal void BeginPass(short v) {
		throw new NotImplementedException();
	}
}