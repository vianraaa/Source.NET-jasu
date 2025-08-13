using Microsoft.Extensions.DependencyInjection;

using Source.Common.Engine;
using Source.Common.MaterialSystem;
using Source.Common.ShaderLib;

using System.Collections.Generic;
using System.Numerics;

namespace Source.MaterialSystem;

public class ShaderRenderState
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

public class ShaderManager : IShaderSystemInternal
{
	List<IShaderDLL> ShaderDLLs = [];
	ShaderRenderState? RenderState;
	byte Modulation;
	byte RenderPass;

	public void BindTexture(Sampler sampler, ITexture texture) {
		throw new NotImplementedException();
	}

	public void DrawElements(IShader shader, Span<IMaterialVar> parms, in ShaderRenderState renderState, VertexCompressionType vertexCompression, uint materialVarTimeStamp) {
		throw new NotImplementedException();
	}

	public IShader? FindShader(ReadOnlySpan<char> shaderName) {
		foreach (var shaderDLL in ShaderDLLs) {
			foreach (var shader in shaderDLL.GetShaders()) {
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

	public ShaderManager() {

	}
	public IServiceProvider Services;
	public void LoadAllShaderDLLs() {
		foreach (var dll in Services.GetServices<IShaderDLL>()) {
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
		"$allowalphatocoverage",
		null
	];

	internal string ShaderStateString(int i) {
		return shaderStateStrings[i];
	}

	internal void InitShaderParameters(IShader shader, IMaterialVar[] vars, string materialName) {
		PrepForShaderDraw(shader, vars, null, 0);
		shader.InitShaderParams(vars, materialName);
		DoneWithShaderDraw();

		if (!vars[(int)ShaderMaterialVars.Color].IsDefined())
			vars[(int)ShaderMaterialVars.Color].SetVecValue(1, 1, 1);

		if (!vars[(int)ShaderMaterialVars.Alpha].IsDefined())
			vars[(int)ShaderMaterialVars.Alpha].SetFloatValue(1);

		int i;
		for (i = shader.GetNumParams(); --i >= 0;) {
			if (vars[i].IsDefined())
				continue;
			ShaderParamType type = shader.GetParamType(i);
			switch (type) {
				case ShaderParamType.Texture:
					// Do nothing; we'll be loading in a string later
					break;
				case ShaderParamType.String:
					// Do nothing; we'll be loading in a string later
					break;
				case ShaderParamType.Material:
					vars[i].SetMaterialValue(null);
					break;
				case ShaderParamType.Bool:
				case ShaderParamType.Integer:
					vars[i].SetIntValue(0);
					break;
				case ShaderParamType.Color:
					vars[i].SetVecValue(1.0f, 1.0f, 1.0f);
					break;
				case ShaderParamType.Vec2:
					vars[i].SetVecValue(0.0f, 0.0f);
					break;
				case ShaderParamType.Vec3:
					vars[i].SetVecValue(0.0f, 0.0f, 0.0f);
					break;
				case ShaderParamType.Vec4:
					vars[i].SetVecValue(0.0f, 0.0f, 0.0f, 0.0f);
					break;
				case ShaderParamType.Float:
					vars[i].SetFloatValue(0);
					break;
				case ShaderParamType.FourCC:
					vars[i].SetFourCCValue(0, 0);
					break;
				case ShaderParamType.Matrix: {
						Matrix4x4 identity = Matrix4x4.Identity;
						vars[i].SetMatrixValue(identity);
					}
					break;
				case ShaderParamType.Matrix4x2: {
						Matrix4x4 identity = Matrix4x4.Identity;
						vars[i].SetMatrixValue(identity);
					}
					break;
				default:
					Dbg.Assert(false);
					break;
			}
		}
	}

	private void DoneWithShaderDraw() {
		RenderState = null;
	}

	private void PrepForShaderDraw(IShader shader, IMaterialVar[] vars, ShaderRenderState? renderState, int modulation) {
		Dbg.Assert(RenderState == null);
		// LATER; plug into spew?
		RenderState = renderState;
		Modulation = (byte)modulation;
		RenderPass = 0;
	}
}