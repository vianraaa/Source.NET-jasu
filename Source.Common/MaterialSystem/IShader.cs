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
	void InitShaderParams(IMaterialVar[] vars, string materialName);
}

public interface IShaderInit {
	public void LoadTexture(IMaterialVar textureVar, ReadOnlySpan<char> textureGroupName, int additionalCreationFlags = 0);
}


public enum VertexCompressionType
{
	None = 0
}

public interface IShaderShadow;
public interface IShaderDynamicAPI;