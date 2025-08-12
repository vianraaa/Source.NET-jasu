using Microsoft.Extensions.DependencyInjection;

using Source.Common.Engine;
using Source.Common.MaterialSystem;

namespace Source.MaterialSystem;

public struct ShaderRenderState
{
	public const int SHADER_OPACITY_ALPHATEST = 0x0010;
	public const int SHADER_OPACITY_OPAQUE = 0x0020;
	public const int SHADER_OPACITY_TRANSLUCENT = 0x0040;
	public const int SHADER_OPACITY_MASK = 0x0070;

	public int Flags;
	public VertexFormat VertexFormat;
	public VertexFormat VertexUsage;

	public bool IsTranslucent() => (Flags & SHADER_OPACITY_TRANSLUCENT) != 0;
	public bool IsAlphaTested() => (Flags & SHADER_OPACITY_ALPHATEST) != 0;
}

public interface IShaderSystemInternal : IShaderSystem
{
	void LoadAllShaderDLLs();
	bool LoadShaderDLL<T>(T instance) where T : IShaderDLL;
	IShader? FindShader(ReadOnlySpan<char> shaderName);

	void DrawElements(IShader shader, Span<IMaterialVar> parms, in ShaderRenderState renderState, VertexCompressionType vertexCompression, uint materialVarTimeStamp);
	IEnumerable<IShader> GetShaders();
}

public class ShaderManager(MaterialSystem materials, IEngineAPI engineAPI) : IShaderSystemInternal
{
	List<IShaderDLL> ShaderDLLs = [];
	public void BindTexture(Sampler sampler, ITexture texture) {
		throw new NotImplementedException();
	}

	public void DrawElements(IShader shader, Span<IMaterialVar> parms, in ShaderRenderState renderState, VertexCompressionType vertexCompression, uint materialVarTimeStamp) {
		throw new NotImplementedException();
	}

	public IShader? FindShader(ReadOnlySpan<char> shaderName) {
		foreach(var shaderDLL in ShaderDLLs) {
			foreach(var shader in shaderDLL.GetShaders()) {
				if (shaderName.Equals(shader.GetName(), StringComparison.OrdinalIgnoreCase))
					return shader;
			}
		}
		return null;
	}

	public IEnumerable<IShader> GetShaders() {
		throw new NotImplementedException();
	}

	public bool LoadShaderDLL<T>(T shaderAPI) where T : IShaderDLL {
		ShaderDLLs.Add(shaderAPI);
		return true;
	}

	public void LoadAllShaderDLLs() {
		foreach(var dll in engineAPI.GetServices<IShaderDLL>()) {
			LoadShaderDLL(dll);
		}
	}

	static string[] shaderStateStrings = [
		"$debug",
		"$no_fullbright",
		"$no_draw",
		"$use_in_fillrate_mode",

		"$vertexcolor",
		"$vertexalpha",
		"$selfillum",
		"$additive",
		"$alphatest",
		"$multipass",
		"$znearer",
		"$model",
		"$flat",
		"$nocull",
		"$nofog",
		"$ignorez",
		"$decal",
		"$envmapsphere",
		"$noalphamod",
		"$envmapcameraspace",
		"$basealphaenvmapmask",
		"$translucent",
		"$normalmapalphaenvmapmask",
		"$softwareskin",
		"$opaquetexture",
		"$envmapmode",
		"$nodecal",
		"$halflambert",
		"$wireframe",
		"$allowalphatocoverage"
	];

	internal string ShaderStateString(int i) {
		return shaderStateStrings[i];
	}

	internal void InitShaderParameters(IShader shader, IMaterialVar[] vars, string v) {
		throw new NotImplementedException();
	}
}