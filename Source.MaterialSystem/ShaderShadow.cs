using Source.Common.MaterialSystem;
using Source.Common.ShaderAPI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Source.MaterialSystem;

/// <summary>
/// A basic representation of the OpenGL state machine
/// </summary>
public struct GraphicsBoardState
{
	public bool Blending;
	public ShaderBlendFactor SourceBlend;
	public ShaderBlendFactor DestinationBlend;
	public ShaderBlendOp BlendOperation;

	public bool DepthTest;
	public bool DepthWrite;
}

/// <summary>
/// Shared uniforms between both types of shaders.
/// </summary>
public struct SourceSharedShadowState
{

}

/// <summary>
/// Uniforms for the vertex shader the ShadowState represents.
/// </summary>
public struct SourceVertexSharedShadowState
{

}

/// <summary>
/// Uniforms for the pixel shader the ShadowState represents.
/// </summary>
public unsafe struct SourcePixelSharedShadowState
{
	public int IsAlphaTesting;
	public int AlphaTestFunc;
	public float AlphaTestRef;
}

/// <summary>
/// A shader state. Represents the board (GL state machine) and shader uniforms together.
/// During shader initialization/recomputes, this state is recalculated based on input variables, etc.
/// </summary>
public class ShadowState : IShaderShadow
{
	internal readonly ShaderSystem Shaders;
	internal readonly ShaderAPIGl46 ShaderAPI;

	public uint BASE_UBO;
	public uint VERTEX_UBO;
	public uint PIXEL_UBO;

	public GraphicsBoardState State;
	public SourceSharedShadowState Base;
	public SourceVertexSharedShadowState Vertex;
	public SourcePixelSharedShadowState Pixel;

	public VertexShaderHandle VertexShader;
	public PixelShaderHandle PixelShader;

	List<IMaterialVar> shaderUniforms = [];

	public void SetShaderUniform(IMaterialVar textureVar) {
		shaderUniforms.Add(textureVar);
	}
	public void ActivateShaderUniforms() {
		foreach(var var in shaderUniforms) {
			ShaderAPI.SetShaderUniform(var);
		}
	}
	private static unsafe int SizeAligned<T>(int alignment = 16) where T : unmanaged {
		var size = sizeof(T);
		var a = alignment - (size % alignment);
		return size + a;
	}

	string? name;
	public unsafe ShadowState(ShaderAPIGl46 shaderAPI, ReadOnlySpan<char> name = default) {
		ShaderAPI = shaderAPI;
		Shaders = (ShaderSystem)shaderAPI.ShaderManager;
		this.name = name == null ? null : new(name);

		if (shaderAPI.IsActive()) {
			CreateShaderObjects();
		}
	}

	bool createdShaderObjects = false;
	private unsafe void CreateShaderObjects() {
		if (createdShaderObjects)
			return;

		BASE_UBO = glCreateBuffer();
		glObjectLabel(GL_BUFFER, BASE_UBO, $"ShaderAPI ShadowState[base] '{name}'");
		glNamedBufferData(BASE_UBO, SizeAligned<SourceSharedShadowState>(), null, GL_DYNAMIC_DRAW);

		VERTEX_UBO = glCreateBuffer();
		glObjectLabel(GL_BUFFER, VERTEX_UBO, $"ShaderAPI ShadowState[vertex] '{name}'");
		glNamedBufferData(VERTEX_UBO, SizeAligned<SourceVertexSharedShadowState>(), null, GL_DYNAMIC_DRAW);

		PIXEL_UBO = glCreateBuffer();
		glObjectLabel(GL_BUFFER, PIXEL_UBO, $"ShaderAPI ShadowState[pixel] '{name}'");
		glNamedBufferData(PIXEL_UBO, SizeAligned<SourcePixelSharedShadowState>(), null, GL_DYNAMIC_DRAW);

		createdShaderObjects = true;
	}

	bool needsBufferUpload = true;
	internal VertexFormat VertexFormat;

	public unsafe void Dispose() {
		if (!ThreadInMainThread()) {
			Warning("NOT IN MAIN THREAD - CANNOT DELETE UBO - GRAPHICS MEMORY LEAK\n");
			return;
		}

		glDeleteBuffers(BASE_UBO, VERTEX_UBO, PIXEL_UBO);
	}

	public unsafe void Activate() {
		CreateShaderObjects(); // Recreate UBO's, if we were lazy-loaded
		ReuploadBuffers(); // Reupload UBO's, if needed

		// Set GL states. We compare our last upload state to the current desired state and adjust if it differs.
		ShaderAPI.SetBoardState(in State);

		// Set VSH and PSH. Shader API can bind these whenever it needs to
		ShaderAPI!.BindVertexShader(in VertexShader);
		ShaderAPI!.BindPixelShader(in PixelShader);

		// Bind UBO binding locations to their respective ranges in our UBO object
		glBindBufferBase(GL_UNIFORM_BUFFER, (int)UniformBufferBindingLocation.SharedBaseShader, BASE_UBO);
		glBindBufferBase(GL_UNIFORM_BUFFER, (int)UniformBufferBindingLocation.SharedVertexShader, VERTEX_UBO);
		glBindBufferBase(GL_UNIFORM_BUFFER, (int)UniformBufferBindingLocation.SharedPixelShader, PIXEL_UBO);

		// Activate per-shader-instance uniforms...
		ActivateShaderUniforms();

		// And now the shader shadow state is activated
	}

	private unsafe void ReuploadBuffers() {
		if (!needsBufferUpload)
			return;

		// Reupload UBO states.
		fixed (SourceSharedShadowState* pBase = &Base)
		fixed (SourceVertexSharedShadowState* pVertex = &Vertex)
		fixed (SourcePixelSharedShadowState* pPixel = &Pixel) {
			glNamedBufferData(BASE_UBO, SizeAligned<SourceSharedShadowState>(), pBase, GL_DYNAMIC_DRAW);
			glNamedBufferData(VERTEX_UBO, SizeAligned<SourceVertexSharedShadowState>(), pVertex, GL_DYNAMIC_DRAW);
			glNamedBufferData(PIXEL_UBO, SizeAligned<SourcePixelSharedShadowState>(), pPixel, GL_DYNAMIC_DRAW);
		}

		needsBufferUpload = false;
	}

	public void DepthFunc(ShaderDepthFunc depthFunc) {
		throw new NotImplementedException();
	}

	public void EnableDepthWrites(bool enable) {
		State.DepthWrite = enable;
	}

	public void EnableDepthTest(bool enable) {
		State.DepthTest = enable;
	}

	public void EnablePolyOffset(PolygonOffsetMode offsetMode) {
		throw new NotImplementedException();
	}

	public void EnableColorWrites(bool enable) {
		throw new NotImplementedException();
	}

	public void EnableAlphaWrites(bool enable) {
		throw new NotImplementedException();
	}

	public void EnableBlending(bool enable) {
		State.Blending = enable;
	}

	public void BlendFunc(ShaderBlendFactor srcFactor, ShaderBlendFactor dstFactor) {
		State.SourceBlend = srcFactor;
		State.DestinationBlend = dstFactor;
	}

	public void EnableAlphaTest(bool enable) {
		int enableI = enable ? 1 : 0;
		if (Pixel.IsAlphaTesting != enableI) {
			Pixel.IsAlphaTesting = enableI;
			needsBufferUpload = true;
		}
	}

	public void AlphaFunc(ShaderAlphaFunc alphaFunc, float alphaRef) {
		int alphaFuncI = (int)alphaFunc;

		if (Pixel.AlphaTestFunc != alphaFuncI) {
			Pixel.AlphaTestFunc = alphaFuncI;
			needsBufferUpload = true;
		}

		if (Pixel.AlphaTestRef != alphaRef) {
			Pixel.AlphaTestRef = alphaRef;
			needsBufferUpload = true;
		}
	}

	public void PolyMode(ShaderPolyModeFace face, ShaderPolyMode polyMode) {
		throw new NotImplementedException();
	}

	public void EnableCulling(bool enable) {
		throw new NotImplementedException();
	}

	public void EnableConstantColor(bool enable) {
		throw new NotImplementedException();
	}

	public void VertexShaderVertexFormat(VertexFormat format, int texCoordCount, Span<int> texCoordDimensions, int userDataSize) {
		VertexFormat = format;
	}

	public GraphicsDriver GetDriver() => ShaderAPI.GetDriver();

	public void SetVertexShader(ReadOnlySpan<char> fileName) {
		VertexShader = Shaders.LoadVertexShader(fileName);
	}

	public void SetPixelShader(ReadOnlySpan<char> fileName) {
		PixelShader = Shaders.LoadPixelShader(fileName);
	}

	public void EnableVertexBlend(bool enable) {
		throw new NotImplementedException();
	}

	public void OverbrightValue(TextureStage stage, float value) {
		throw new NotImplementedException();
	}

	bool[] samplerState = new bool[(int)Sampler.MaxSamplers];

	public void EnableTexture(Sampler sampler, bool enable) {
		if ((int)sampler < 16) {
			samplerState[(int)sampler] = enable;
		}
		else {
			Warning($"Attempting to bind a texture to an invalid sampler {(int)sampler}!\n");
		}
	}

	public void EnableTexGen(TextureStage stage, bool enable) {
		throw new NotImplementedException();
	}

	public void TexGen(TextureStage stage, ShaderTexGenParam param) {
		throw new NotImplementedException();
	}

	public void EnableCustomPixelPipe(bool enable) {
		throw new NotImplementedException();
	}

	public void CustomTextureStages(int stageCount) {
		throw new NotImplementedException();
	}

	public void CustomTextureOperation(TextureStage stage, ShaderTexChannel channel, ShaderTexOp op, ShaderTexArg arg1, ShaderTexArg arg2) {
		throw new NotImplementedException();
	}

	public void EnableAlphaPipe(bool enable) {
		throw new NotImplementedException();
	}

	public void EnableConstantAlpha(bool enable) {
		throw new NotImplementedException();
	}

	public void EnableVertexAlpha(bool enable) {
		throw new NotImplementedException();
	}

	public void EnableTextureAlpha(TextureStage stage, bool enable) {
		throw new NotImplementedException();
	}

	public void EnableBlendingSeparateAlpha(bool enable) {
		throw new NotImplementedException();
	}

	public void BlendFuncSeparateAlpha(ShaderBlendFactor srcFactor, ShaderBlendFactor dstFactor) {
		throw new NotImplementedException();
	}

	public void FogMode(ShaderFogMode fogMode) {
		throw new NotImplementedException();
	}

	public void SetDiffuseMaterialSource(ShaderMaterialSource materialSource) {
		throw new NotImplementedException();
	}

	public void DisableFogGammaCorrection(bool bDisable) {
		throw new NotImplementedException();
	}

	public void EnableAlphaToCoverage(bool enable) {
		throw new NotImplementedException();
	}

	public void SetShadowDepthFiltering(Sampler stage) {
		throw new NotImplementedException();
	}

	public void BlendOp(ShaderBlendOp blendOp) {
		throw new NotImplementedException();
	}

	public void BlendOpSeparateAlpha(ShaderBlendOp blendOp) {
		throw new NotImplementedException();
	}
}