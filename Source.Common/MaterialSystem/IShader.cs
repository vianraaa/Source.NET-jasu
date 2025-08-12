namespace Source.Common.MaterialSystem;

public interface IShader
{
	string? GetName();

	int GetNumParams();
	ReadOnlySpan<char> GetParamName(int paramIndex);
	ReadOnlySpan<char> GetParamHelp(int paramIndex);
	ShaderParamType GetParamType(int paramIndex);
    ReadOnlySpan<char> GetParamDefault(int paramIndex);
	string GetFallbackShader(IMaterialVar[] vars);
}
