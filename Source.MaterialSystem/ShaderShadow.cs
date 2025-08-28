using Source.Common.MaterialSystem;
using Source.Common.ShaderAPI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Source.MaterialSystem;


public struct SourceVertexSharedShadowState
{

}

public unsafe struct SourcePixelSharedShadowState
{
	public int IsAlphaTesting;
	public int AlphaTestFunc;
	public float AlphaTestRef;
}

public struct SourceSharedShadowState
{
	
}

public class ShadowState : IShaderShadow
{
	internal readonly ShaderSystem Shaders;
	internal readonly ShaderAPIGl46 ShaderAPI;

	public uint UBO;
	public SourceSharedShadowState Base;
	public SourceVertexSharedShadowState Vertex;
	public SourcePixelSharedShadowState Pixel;

	public VertexShaderHandle VertexShader;
	public PixelShaderHandle PixelShader;

	public unsafe ShadowState(ShaderAPIGl46 shaderAPI, ReadOnlySpan<char> name = default) {
		ShaderAPI = shaderAPI;
		Shaders = (ShaderSystem)shaderAPI.ShaderManager;
		UBO = glCreateBuffer();
		glObjectLabel(GL_BUFFER, UBO, $"ShaderAPI ShadowState UBO '{name}'");
		glNamedBufferData(UBO, sizeof(SourceSharedShadowState) + sizeof(SourceVertexSharedShadowState) + sizeof(SourcePixelSharedShadowState), null, GL_DYNAMIC_DRAW);
	}

	bool needsBufferUpload = true;
	internal VertexFormat VertexFormat;

	public unsafe void Dispose() {
		if (!ThreadInMainThread()) {
			Warning("NOT IN MAIN THREAD - CANNOT DELETE UBO - GRAPHICS MEMORY LEAK\n");
			return;
		}

		glDeleteBuffers(UBO);
	}

	private unsafe int LOC_BASE => 0;
	private unsafe int LOC_VERTEX => sizeof(SourceSharedShadowState);
	private unsafe int LOC_PIXEL => sizeof(SourceSharedShadowState) + sizeof(SourceVertexSharedShadowState);

	public unsafe void Activate() {
		ReuploadBuffers(); // Reupload UBO's, if needed
		
		// Set GL states
		// TODO; Can we trust the driver will be smart enough not to update states when needed or should we do that

		// Set VSH and PSH. Shader API can bind these whenever it needs to
		ShaderAPI!.BindVertexShader(in VertexShader);
		ShaderAPI!.BindPixelShader(in PixelShader);

		// Bind UBO binding locations to their respective ranges in our UBO object
		glBindBufferRange(GL_UNIFORM_BUFFER, (int)UniformBufferBindingLocation.SharedBaseShader, UBO, LOC_BASE, sizeof(SourceSharedShadowState));
		glBindBufferRange(GL_UNIFORM_BUFFER, (int)UniformBufferBindingLocation.SharedVertexShader, UBO, LOC_VERTEX, sizeof(SourceVertexSharedShadowState));
		glBindBufferRange(GL_UNIFORM_BUFFER, (int)UniformBufferBindingLocation.SharedPixelShader, UBO, LOC_PIXEL, sizeof(SourcePixelSharedShadowState));

		// And now the shader shadow state is activated
	}

	public unsafe void ReuploadBuffers() {
		if (!needsBufferUpload)
			return;

		// Reupload UBO states.
		fixed (SourceSharedShadowState* pBase = &Base)
		fixed (SourceVertexSharedShadowState* pVertex = &Vertex)
		fixed (SourcePixelSharedShadowState* pPixel = &Pixel) {
			glNamedBufferSubData(UBO, LOC_BASE, sizeof(SourceSharedShadowState), pBase);
			glNamedBufferSubData(UBO, LOC_VERTEX, sizeof(SourceVertexSharedShadowState), pVertex);
			glNamedBufferSubData(UBO, LOC_PIXEL, sizeof(SourcePixelSharedShadowState), pPixel);
		}

		needsBufferUpload = false;
	}

	public void DepthFunc(ShaderDepthFunc depthFunc) {
		throw new NotImplementedException();
	}

	public void EnableDepthWrites(bool enable) {
		throw new NotImplementedException();
	}

	public void EnableDepthTest(bool enable) {
		throw new NotImplementedException();
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
		throw new NotImplementedException();
	}

	public void BlendFunc(ShaderBlendFactor srcFactor, ShaderBlendFactor dstFactor) {
		throw new NotImplementedException();
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
		if((int)sampler < 16) {
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