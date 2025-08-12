using Source.Common.MaterialSystem;

namespace Source.MaterialSystem;

public struct RenderPassList {
	public const int MAX_RENDER_PASSES = 4;
	public int PassCount;
}

public struct ShaderRenderState {
	public int Flags;
	public VertexFormat VertexFormat;
	public VertexFormat VertexUsage;
}

public interface IShaderSystemInternal : IShaderSystem
{
	IShader? FindShader(ReadOnlySpan<char> shaderName);
	void DrawElements(IShader shader, Span<IMaterialVar> parms, in ShaderRenderState renderState);
}

public class ShaderManager : IShaderSystemInternal
{
	public void BindTexture(Sampler sampler, ITexture texture) {
		throw new NotImplementedException();
	}

	public void DrawElements(IShader shader, Span<IMaterialVar> parms, in ShaderRenderState renderState) {
		throw new NotImplementedException();
	}

	public IShader? FindShader(ReadOnlySpan<char> shaderName) {
		throw new NotImplementedException();
	}
}