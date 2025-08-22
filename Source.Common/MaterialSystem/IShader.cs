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
}