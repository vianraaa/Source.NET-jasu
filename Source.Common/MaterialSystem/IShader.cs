using Source.Common.ShaderAPI;
using Source.Common.ShaderLib;

namespace Source.Common.MaterialSystem;


public struct ShaderParamInfo
{
	public string Name;
	public string Help;
	public ShaderParamType Type;
	public string? DefaultValue;
	public ShaderParamFlags Flags;
}


public interface IShader
{
	string? GetName();
	int GetFlags();
	int GetNumParams();
	ReadOnlySpan<char> GetParamName(int paramIndex);
	ReadOnlySpan<char> GetParamHelp(int paramIndex);
	ShaderParamType GetParamType(int paramIndex);
    ReadOnlySpan<char> GetParamDefault(int paramIndex);
	string? GetFallbackShader(IMaterialVar[] vars);
	void InitShaderParams(IMaterialVar[] vars, ReadOnlySpan<char> materialName);
	void InitShaderInstance(IMaterialVar[] shaderParams, IShaderInit shaderManager, ReadOnlySpan<char> materialName, ReadOnlySpan<char> textureGroupName);
	void DrawElements(IMaterialVar[] shaderParams, int modulationFlags, IShaderShadow shadow, IShaderDynamicAPI shaderAPI, VertexCompressionType none, ref BasePerMaterialContextData basePerMaterialContextData);
	int ComputeModulationFlags(Span<IMaterialVar> parms, IShaderAPI shaderAPI);
}

public interface IShaderInit {
	public void LoadTexture(IMaterialVar textureVar, ReadOnlySpan<char> textureGroupName, int additionalCreationFlags = 0);
}


public enum VertexCompressionType : uint
{
	Invalid = 0xFFFFFFFF,
	None = 0,
	On = 1
}

public struct ShaderViewport {
	public int TopLeftX;
	public int TopLeftY;
	public int Width;
	public int Height;
	public float MinZ;
	public float MaxZ;

	public ShaderViewport() {

	}

	public ShaderViewport(int x, int y, int width, int height, float minZ = 0.0f, float maxZ = 1.0f) {
		TopLeftX = x;
		TopLeftY = y;
		Width = width;
		Height = height;
		MinZ = minZ;
		MaxZ = maxZ;
	}
}

public interface IShaderDynamicAPI
{
	int GetCurrentNumBones();
	MaterialFogMode GetSceneFogMode();
	bool InFlashlightMode();
	void PushMatrix();
	void PopMatrix();
	void SetPixelShaderConstant(int v1, Span<float> flConsts, int v2);
	void SetVertexShaderIndex(int value);
	IMesh GetDynamicMesh(IMaterial material, int nCurrentBoneCount, bool buffered, IMesh? vertexOverride, IMesh? indexOverride);
	bool InEditorMode();
	void SetVertexShaderConstant(int vERTEX_SHADER_MODULATION_COLOR, Span<float> color);
}