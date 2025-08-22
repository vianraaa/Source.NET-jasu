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

public unsafe struct DynamicState {
	public int NumBones;
}

public class ShaderShadowGl46 : IShaderShadow
{
	public MeshMgr MeshMgr;
	// Separate alpha control?
	bool AlphaPipe;

	// Constant color state
	bool hasConstantColor;
	bool hasConstantAlpha;

	// Vertex color state
	bool hasVertexAlpha;

	// funky custom method of specifying shader state
	bool customTextureStageState;

	// Number of stages used by the custom pipeline
	int customTextureStages;

	// Number of bones...
	int numBlendVertices;

	// Draw flags
	int drawFlags;

	// Alpha blending...
	ShaderBlendFactor srcBlend;
	ShaderBlendFactor destBlend;
	ShaderBlendOp blendOp;

	// GR - Separate alpha blending...
	ShaderBlendFactor srcBlendAlpha;
	ShaderBlendFactor destBlendAlpha;
	ShaderBlendOp blendOpAlpha;

	// Alpha testing
	ShaderAlphaFunc alphaFunc;
	int alphaRef;

	// Stencil
	ShaderStencilFunc stencilFunc;
	int stencilRef;
	int stencilMask;
	ShaderStencilOp stencilFail;
	ShaderStencilOp stencilZFail;
	ShaderStencilOp stencilPass;
	int stencilWriteMask;

	ShadowState ShadowState;
	ShadowShaderState ShadowShaderState;

	TextureStageState[] TextureStage = new TextureStageState[ShadowState.MAX_TEXTURE_STAGES];
	SamplerState[] SamplerStage = new SamplerState[ShadowState.MAX_SAMPLERS];

	public HardwareConfig HardwareConfig;
	public ref ShadowState GetShadowState() {
		return ref ShadowState;
	}
	public ref ShadowShaderState GetShadowShaderState() {
		return ref ShadowShaderState;
	}
	public void ComputeAggregateShadowState() {
		uint flags = 0;

		// Initialize the texture stage usage; this may get changed later
		for (int i = 0; i < HardwareConfig.GetSamplerCount(); ++i) {
			ShadowState.SamplerState[i].TextureEnable = IsUsingTextureCoordinates((Sampler)i);

			// Deal with the alpha pipe
			if (ShadowState.UsingFixedFunction && AlphaPipe) {
				if (TextureStage[i].TextureAlphaEnable) {
					ShadowState.SamplerState[i].TextureEnable = true;
				}
			}
		}

		// Always use the same alpha src + dest if it's disabled
		// NOTE: This is essential for stateblocks to work
		if (ShadowState.AlphaBlendEnable) {
			ShadowState.SrcBlend = (uint)srcBlend;
			ShadowState.DestBlend = (uint)destBlend;
			ShadowState.BlendOp = (uint)blendOp;
		}
		else {
			ShadowState.SrcBlend = (uint)ShaderBlendFactor.One;
			ShadowState.DestBlend = (uint)ShaderBlendFactor.Zero;
			ShadowState.BlendOp = (uint)ShaderBlendOp.Add;
		}

		// GR
		if (ShadowState.SeparateAlphaBlendEnable) {
			ShadowState.SrcBlendAlpha = (uint)srcBlendAlpha;
			ShadowState.DestBlendAlpha = (uint)destBlendAlpha;
			ShadowState.BlendOpAlpha = (uint)blendOpAlpha;
		}
		else {
			ShadowState.SrcBlendAlpha = (uint)ShaderBlendFactor.One;
			ShadowState.DestBlendAlpha = (uint)ShaderBlendFactor.Zero;
			ShadowState.BlendOpAlpha = (uint)ShaderBlendOp.Add;
		}

		// Use the same func if it's disabled
		if (ShadowState.AlphaTestEnable) {
			// If alpha test is enabled, just use the values set
			ShadowState.AlphaFunc = (uint)alphaFunc;
			ShadowState.AlphaRef = (uint)alphaRef;
		}
		else {
			// A default value
			ShadowState.AlphaFunc = (uint)ShaderAlphaFunc.GreaterEqual;
			ShadowState.AlphaRef = 0;

			// If not alpha testing and doing a standard alpha blend, force on alpha testing
			if (ShadowState.AlphaBlendEnable) {
				if ((ShadowState.SrcBlend == (uint)ShaderBlendFactor.SrcAlpha) && (ShadowState.DestBlend == (uint)ShaderBlendFactor.InvSrcAlpha)) {
					ShadowState.AlphaFunc = (uint)ShaderAlphaFunc.GreaterEqual;
					ShadowState.AlphaRef = 1;
				}
			}
		}
		if (ShadowState.UsingFixedFunction) {
			flags = (uint)drawFlags;

			// We need to take this bad boy into account
			// Or do we TODO REVIEW
			// if (HasConstantColor())
			// flags |= SHADER_HAS_CONSTANT_COLOR;

			// We need to take lighting into account..
			if (ShadowState.Lighting)
				flags |= (uint)(ShaderDrawBitField.Normal | ShaderDrawBitField.Color);

			// Look for inconsistency in the shadow state (can't have texgen &
			// SHADER_DRAW_TEXCOORD or SHADER_DRAW_SECONDARY_TEXCOORD0 on the same stage)
			if ((flags & (uint)(ShaderDrawBitField.TexCoord0 | ShaderDrawBitField.SecondaryTexCoord0)) != 0) {
				Assert((ShadowState.TextureStage[0].TexCoordIndex & 0xFFFF0000) == 0);
			}
			if ((flags & (uint)(ShaderDrawBitField.TexCoord1 | ShaderDrawBitField.SecondaryTexCoord1)) != 0) {
				Assert((ShadowState.TextureStage[1].TexCoordIndex & 0xFFFF0000) == 0);
			}
			if ((flags & (uint)(ShaderDrawBitField.TexCoord2 | ShaderDrawBitField.SecondaryTexCoord2)) != 0) {
				Assert((ShadowState.TextureStage[2].TexCoordIndex & 0xFFFF0000) == 0);
			}
			if ((flags & (uint)(ShaderDrawBitField.TexCoord3 | ShaderDrawBitField.SecondaryTexCoord3)) != 0) {
				Assert((ShadowState.TextureStage[3].TexCoordIndex & 0xFFFF0000) == 0);
			}

			// Vertex usage has already been set for pixel + vertex shaders
			ShadowShaderState.VertexUsage = FlagsToVertexFormat(flags);

			// Configure the texture stages
			ConfigureFVFVertexShader(flags);
		}
		else {
			// Pixel shaders, disable everything so as to prevent unnecessary state changes....
			// Removed i don't think it's applicable here
			ShadowState.Lighting = false;
			ShadowState.SpecularEnable = false;
			ShadowState.VertexBlendEnable = false;
			ShadowShaderState.ModulateConstantColor = false;
		}

		// Compute texture coordinates
		ConfigureTextureCoordinates(flags);

		// Alpha to coverage
		if (ShadowState.EnableAlphaToCoverage) {
			// Only allow this to be enabled if blending is disabled and testing is enabled
			if ((ShadowState.AlphaBlendEnable == true) || (ShadowState.AlphaTestEnable == false)) {
				ShadowState.EnableAlphaToCoverage = false;
			}
		}
	}

	private void ConfigureFVFVertexShader(uint flags) {

	}

	private void ConfigureTextureCoordinates(uint flags) {
		for (int i = 0; i < HardwareConfig.GetTextureStageCount(); ++i) {
			TextureCoordinate((Common.MaterialSystem.TextureStage)i, i);
		}

		if ((flags & (uint)ShaderDrawBitField.TexCoord0) != 0) {
			Assert((flags & (uint)ShaderDrawBitField.LightmapTexCoord0) == 0);
			TextureCoordinate(Common.MaterialSystem.TextureStage.Stage0, 0);
		}
		else if ((flags & (uint)ShaderDrawBitField.LightmapTexCoord0) != 0) {
			TextureCoordinate(Common.MaterialSystem.TextureStage.Stage0, 1);
		}
		else if ((flags & (uint)ShaderDrawBitField.SecondaryTexCoord0) != 0) {
			TextureCoordinate(Common.MaterialSystem.TextureStage.Stage0, 2);
		}

		if ((flags & (uint)ShaderDrawBitField.TexCoord1) != 0) {
			Assert((flags & (uint)ShaderDrawBitField.LightmapTexCoord1) == 0);
			TextureCoordinate(Common.MaterialSystem.TextureStage.Stage0, 0);
		}
		else if ((flags & (uint)ShaderDrawBitField.LightmapTexCoord1) != 0) {
			TextureCoordinate(Common.MaterialSystem.TextureStage.Stage1, 1);
		}
		else if ((flags & (uint)ShaderDrawBitField.SecondaryTexCoord1) != 0) {
			TextureCoordinate(Common.MaterialSystem.TextureStage.Stage1, 2);
		}
	}

	private void TextureCoordinate(TextureStage stage, int useTexCoord) {
		if ((int)stage < HardwareConfig.GetTextureStageCount()) {
			TextureStage[(int)stage].TexCoordinate = useTexCoord;

			// Need to recompute the texCoordIndex, since that's affected by this
			RecomputeTexCoordIndex(stage);
		}
	}

	private void RecomputeTexCoordIndex(TextureStage stage) {
		int texCoordIndex = TextureStage[(int)stage].TexCoordinate;
		if (TextureStage[(int)stage].TexGenEnable)
			texCoordIndex |= TextureStage[(int)stage].TexCoordIndex;
		ShadowState.TextureStage[(int)stage].TexCoordIndex = texCoordIndex;
	}

	private VertexFormat FlagsToVertexFormat(uint flags) {
		// Flags -1 occurs when there's an error condition;
		// we'll just give em the max space and let them fill it in.
		int formatFlags = 0;
		Span<int> texCoordSize = [0, 0, 0, 0, 0, 0, 0, 0];
		int userDataSize = 0;
		int numBones = 0;

		// Flags -1 occurs when there's an error condition;
		// we'll just give em the max space and let them fill it in.
		if (flags == -1) {
			formatFlags = VertexFormatFlags.VertexFormatPosition | VertexFormatFlags.VertexFormatNormal | VertexFormatFlags.VertexFormatColor |
				VertexFormatFlags.VertexFormatTangentS | VertexFormatFlags.VertexFormatTangentT;
			texCoordSize[0] = texCoordSize[1] = texCoordSize[2] = 2;
		}
		else {
			if ((flags & (uint)ShaderDrawBitField.Position) != 0)
				formatFlags |= VertexFormatFlags.VertexFormatPosition;

			if ((flags & (uint)ShaderDrawBitField.Normal) != 0)
				formatFlags |= VertexFormatFlags.VertexFormatNormal;

			if ((flags & (uint)ShaderDrawBitField.Color) != 0)
				formatFlags |= VertexFormatFlags.VertexFormatColor;

			if ((flags & (uint)ShaderDrawBitField.Specular) != 0)
				formatFlags |= VertexFormatFlags.VertexFormatSpecular;

			if ((flags & (uint)ShaderDrawBitField.TexCoordMask) != 0)
				// normal texture coords into texture 0
				texCoordSize[0] = 2;


			if ((flags & (uint)ShaderDrawBitField.LightmapTexCoordMask) != 0)
				// lightmaps go into texcoord 1
				texCoordSize[1] = 2;


			if ((flags & (uint)ShaderDrawBitField.SecondaryTexCoordMask) != 0)
				// any texgen, or secondary texture coordinate is put into texcoord 2
				texCoordSize[2] = 2;
		}

		// Hardware skinning...	always store space for up to 3 bones
		// and always assume index blend enabled if available
		if (ShadowState.VertexBlendEnable) {
			if (HardwareConfig.MaxBlendMatrixIndices() > 0)
				formatFlags |= VertexFormatFlags.VertexFormatBoneIndex;

			if (HardwareConfig.MaxBlendMatrices() > 2)
				numBones = 2;   // the third bone weight is implied
			else
				numBones = HardwareConfig.MaxBlendMatrices() - 1;
		}

		return MeshMgr.ComputeVertexFormat(formatFlags, IMesh.VERTEX_MAX_TEXTURE_COORDINATES,
			texCoordSize, numBones, userDataSize);
	}

	private bool HasConstantColor() {
		return hasConstantColor;
	}

	private bool IsUsingTextureCoordinates(Sampler i) {
		return SamplerStage[(int)i].TextureEnable;
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
		if(DynamicState.NumBones != bones) {
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

	public enum TransformType {
		IsIdentity = 0,
		IsCameraToWorld,
		IsGeneral
	}

	internal void BeginPass(short v) {
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
		throw new NotImplementedException();
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
}