using Microsoft.Extensions.DependencyInjection;

using Source.Common;
using Source.Common.Engine;
using Source.Common.Filesystem;
using Source.Common.MaterialSystem;
using Source.Common.ShaderLib;

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Text;

namespace Source.MaterialSystem;

public interface IShaderSystemInternal : IShaderInit, IShaderSystem
{
	void LoadAllShaderDLLs();
	bool LoadShaderDLL<T>(T instance) where T : IShaderDLL;
	IShader? FindShader(ReadOnlySpan<char> shaderName);

	void InitShaderParameters(IShader shader, IMaterialVar[] vars, ReadOnlySpan<char> materialName, ReadOnlySpan<char> textureGroupName);
	bool InitRenderState(IShader shader, IMaterialVar[] shaderParams, ref ShadowState shaderRenderState, ReadOnlySpan<char> materialName);
	// void CleanupRenderState(ref ShaderRenderState renderState);
	void DrawElements(IShader shader, IMaterialVar[] parms, in ShadowState renderState, VertexCompressionType vertexCompression, uint materialVarTimeStamp);
	IEnumerable<IShader> GetShaders();
}

public class ShaderSystem : IShaderSystemInternal
{
	List<IShaderDLL> ShaderDLLs = [];
	ShadowState? RenderState;
	internal MaterialSystem MaterialSystem;
	internal ShaderAPIGl46 ShaderAPI;
	internal MaterialSystem_Config Config;

	public void BindTexture(Sampler sampler, ITexture texture, int frame) {
		if (texture == null) return;

		((ITextureInternal)texture).Bind(sampler, frame);
	}

	public void ResetShaderState() {

	}

	public void DrawElements(IShader shader, IMaterialVar[] parms, in ShadowState renderState, VertexCompressionType vertexCompression, uint materialVarTimeStamp) {
		ShaderAPI.InvalidateDelayedShaderConstraints();

		int materialVarFlags = parms[(int)ShaderMaterialVars.Flags].GetIntValue();
		if (((materialVarFlags & (int)MaterialVarFlags.Model) != 0) || (IsFlag2Set(parms, MaterialVarFlags2.SupportsHardwareSkinning) && (ShaderAPI.GetCurrentNumBones() > 0))) {
			ShaderAPI.SetSkinningMatrices();
		}

		if ((Config.ShowNormalMap || Config.ShowMipLevels == 2) && (IsFlag2Set(parms, MaterialVarFlags2.LightingBumpedLightmap) || IsFlag2Set(parms, MaterialVarFlags2.DiffuseBumpmappedModel))) {
			DrawNormalMap(shader, parms, vertexCompression);
		}
		else {
			ShaderAPI.SetDefaultState();

			if ((materialVarFlags & (uint)MaterialVarFlags.Flat) > 0)
				ShaderAPI.ShadeMode(ShadeMode.Flat);

			PrepForShaderDraw(shader, parms, renderState);

			shader.DrawElements(parms, null, ShaderAPI, vertexCompression);
			DoneWithShaderDraw();
		}
	}

	private void DrawNormalMap(IShader shader, Span<IMaterialVar> parms, VertexCompressionType vertexCompression) {
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
		foreach (var shaderDLL in ShaderDLLs) {
			foreach (var shader in shaderDLL.GetShaders())
				yield return shader;
		}
	}

	public bool LoadShaderDLL<T>(T shaderAPI) where T : IShaderDLL {
		ShaderDLLs.Add(shaderAPI);
		return true;
	}

	public ShaderSystem() {

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

	public void InitShaderParameters(IShader shader, IMaterialVar[] vars, ReadOnlySpan<char> materialName, ReadOnlySpan<char> textureGroupName) {
		PrepForShaderDraw(shader, vars, null);
		shader.InitShaderParams(vars, ShaderAPI, materialName);
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

	private void PrepForShaderDraw(IShader shader, Span<IMaterialVar> vars, ShadowState? renderState) {
		Assert(RenderState == null);
		// LATER; plug into spew?
		RenderState = renderState;
		renderState?.Activate(); // Activate the render state, this flushes out UBO's etc
	}

	public void InitShaderInstance(IShader shader, IMaterialVar[] shaderParams, ReadOnlySpan<char> materialName, ReadOnlySpan<char> textureGroupName) {
		PrepForShaderDraw(shader, shaderParams, null);
		shader.InitShaderInstance(shaderParams, ShaderAPI, this, materialName, textureGroupName);
		DoneWithShaderDraw();
	}

	public void LoadTexture(IMaterialVar textureVar, ReadOnlySpan<char> textureGroupName, int additionalCreationFlags = 0) {
		if (textureVar.GetVarType() != MaterialVarType.String) {
			if (textureVar.GetVarType() != MaterialVarType.Texture)
				textureVar.SetTextureValue(MaterialSystem.TextureSystem.ErrorTexture());
			return;
		}

		ReadOnlySpan<char> name = textureVar.GetStringValue();
		if (name[0] == Path.PathSeparator || name[1] == Path.PathSeparator)
			name = name[1..];

		ITextureInternal texture = (ITextureInternal)MaterialSystem.FindTexture(name, textureGroupName, false, additionalCreationFlags);

		if (texture == null) {
			if (!MaterialSystem.ShaderDevice.IsUsingGraphics())
				Warning("Shader_t::LoadTexture: texture \"{name}.vtf\" doesn't exist\n");
			texture = MaterialSystem.TextureSystem.ErrorTexture();
		}

		textureVar.SetTextureValue(texture);
	}

	public bool InitRenderState(IShader shader, IMaterialVar[] shaderParams, ref ShadowState renderState, ReadOnlySpan<char> materialName) {
		Assert(RenderState == null);
		InitRenderStateFlags(ref renderState, shaderParams);
		InitState(shader, shaderParams, ref renderState);
		return true;
	}

	private void InitState(IShader shader, IMaterialVar[] shaderParams, ref ShadowState renderState) {
		PrepForShaderDraw(shader, shaderParams, renderState);
		shader.DrawElements(shaderParams, renderState, null, VertexCompressionType.None);
		DoneWithShaderDraw();
	}

	public const int SNAPSHOT_COUNT_NORMAL = 16;
	public const int SNAPSHOT_COUNT_EDITOR = 32;
	public int SnapshotTypeCount() => MaterialSystem.CanUseEditorMaterials() ? SNAPSHOT_COUNT_EDITOR : SNAPSHOT_COUNT_NORMAL;

	private void InitRenderStateFlags(ref ShadowState renderState, IMaterialVar[] shaderParams) {

	}

	public void Draw(bool makeActualDrawCall = true) {
		Assert(RenderState);

		if (makeActualDrawCall)
			ShaderAPI.RenderPass();

		ShaderAPI.InvalidateDelayedShaderConstraints();
	}

	internal void BindVertexShader(in VertexShaderHandle vertexShader) {

	}

	internal void BindPixelShader(in PixelShaderHandle pixelShader) {

	}

	internal void SetVertexShaderState(int index) {

	}

	internal void SetPixelShaderState(int index) {

	}

	public void Init() {

	}

	Dictionary<ulong, VertexShaderHandle> vshs = [];
	Dictionary<ulong, PixelShaderHandle> pshs = [];

	internal static unsafe bool IsValidShader(uint shader, [NotNullWhen(false)] out string? error) {
		int status = 0;
		glGetShaderiv(shader, GL_COMPILE_STATUS, &status);
		if (status != GL_TRUE) {
			int logLength = 0;
			glGetShaderiv(shader, GL_INFO_LOG_LENGTH, &logLength);
			if (logLength > 0) {
				byte[] infoLog = new byte[logLength];
				fixed (byte* infoPtr = infoLog) {
					glGetShaderInfoLog(shader, logLength, null, infoPtr);
				}
				error = Encoding.ASCII.GetString(infoLog);
			}
			else
				error = "UNKNOWN FAILURE";

			glDeleteShader(shader);
			return false;
		}

		error = null;
		return true;
	}

	internal static unsafe bool IsValidProgram(uint program, [NotNullWhen(false)] out string? error) {
		int status = 0;
		glGetProgramiv(program, GL_LINK_STATUS, &status);
		if (status != GL_TRUE) {
			int logLength = 0;
			glGetProgramiv(program, GL_INFO_LOG_LENGTH, &logLength);
			if (logLength > 0) {
				byte[] infoLog = new byte[logLength];
				fixed (byte* infoPtr = infoLog) {
					glGetProgramInfoLog(program, logLength, null, infoPtr);
				}
				error = Encoding.ASCII.GetString(infoLog);
			}
			else
				error = "UNKNOWN FAILURE";

			glDeleteProgram(program);
			return false;
		}

		error = null;
		return true;
	}

	public unsafe VertexShaderHandle LoadVertexShader(ReadOnlySpan<char> name) {
		ulong symbol = name.Hash();
		if (vshs.TryGetValue(symbol, out VertexShaderHandle value))
			return value;

		using IFileHandle? handle = MaterialSystem.FileSystem.Open($"shaders/{name}", FileOpenOptions.Read, "game");
		if (handle == null)
			return VertexShaderHandle.INVALID;

		Span<byte> source = stackalloc byte[(int)handle.Stream.Length];
		int read = handle.Stream.Read(source);
		uint pShader = 0;
		pShader = glCreateShader(GL_VERTEX_SHADER);
		int len = source.Length;
		fixed (byte* pSrc = source)
			glShaderSource(pShader, 1, &pSrc, &len);
		glCompileShader(pShader);

		if (!IsValidShader(pShader, out string? error)) {
			Warning("WARNING: Vertex shader compilation error.\n");
			Warning(error);
			return VertexShaderHandle.INVALID;
		}

		VertexShaderHandle vsh = new((nint)pShader);
		vshs[symbol] = vsh;
		return vsh;
	}

	public unsafe PixelShaderHandle LoadPixelShader(ReadOnlySpan<char> name) {
		ulong symbol = name.Hash();
		if (pshs.TryGetValue(symbol, out PixelShaderHandle value))
			return value;

		using IFileHandle? handle = MaterialSystem.FileSystem.Open($"shaders/{name}", FileOpenOptions.Read, "game");
		if (handle == null)
			return PixelShaderHandle.INVALID;

		Span<byte> source = stackalloc byte[(int)handle.Stream.Length];
		int read = handle.Stream.Read(source);
		uint pShader = 0;
		pShader = glCreateShader(GL_FRAGMENT_SHADER);
		int len = source.Length;
		fixed (byte* pSrc = source)
			glShaderSource(pShader, 1, &pSrc, &len);
		glCompileShader(pShader);

		if (!IsValidShader(pShader, out string? error)) {
			Warning("WARNING: Pixel shader compilation error.\n");
			Warning(error);
			return PixelShaderHandle.INVALID;
		}

		PixelShaderHandle psh = new((nint)pShader);
		pshs[symbol] = psh;
		return psh;
	}

}