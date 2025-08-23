using Microsoft.Extensions.DependencyInjection;

using Source.Common;
using Source.Common.Engine;
using Source.Common.Filesystem;
using Source.Common.MaterialSystem;
using Source.Common.ShaderLib;

using System.Collections.Generic;
using System.Diagnostics;
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

public interface IShaderSystemInternal : IShaderInit, IShaderSystem
{
	void LoadAllShaderDLLs();
	bool LoadShaderDLL<T>(T instance) where T : IShaderDLL;
	IShader? FindShader(ReadOnlySpan<char> shaderName);

	void InitShaderParameters(IShader shader, IMaterialVar[] vars, ReadOnlySpan<char> materialName, ReadOnlySpan<char> textureGroupName);
	bool InitRenderState(IShader shader, IMaterialVar[] shaderParams, ref ShaderRenderState shaderRenderState, ReadOnlySpan<char> materialName);
	// void CleanupRenderState(ref ShaderRenderState renderState);
	void DrawElements(IShader shader, IMaterialVar[] parms, in ShaderRenderState renderState, VertexCompressionType vertexCompression, uint materialVarTimeStamp);
	IEnumerable<IShader> GetShaders();
}

public class ShaderSystem : IShaderSystemInternal
{
	List<IShaderDLL> ShaderDLLs = [];
	ShaderRenderState? RenderState;
	internal MaterialSystem MaterialSystem;
	internal ShaderAPIGl46 ShaderAPI;
	internal MaterialSystem_Config Config;

	public void BindTexture(Sampler sampler, ITexture texture) {
		throw new NotImplementedException();
	}

	public void ResetShaderState() {

		SetVertexShader(VertexShaderHandle.INVALID);
		SetPixelShader(PixelShaderHandle.INVALID);
	}

	public void DrawElements(IShader shader, IMaterialVar[] parms, in ShaderRenderState renderState, VertexCompressionType vertexCompression, uint materialVarTimeStamp) {
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

			shader.DrawElements(parms, ShaderAPI, vertexCompression);
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

	private void PrepForShaderDraw(IShader shader, Span<IMaterialVar> vars, ShaderRenderState? renderState) {
		Assert(RenderState == null);
		// LATER; plug into spew?
		RenderState = renderState;
	}

	public void InitShaderInstance(IShader shader, IMaterialVar[] shaderParams, ReadOnlySpan<char> materialName, ReadOnlySpan<char> textureGroupName) {
		PrepForShaderDraw(shader, shaderParams, null);
		shader.InitShaderInstance(shaderParams, ShaderAPI, this, materialName, textureGroupName);
		DoneWithShaderDraw();
	}

	public void LoadTexture(IMaterialVar textureVar, ReadOnlySpan<char> textureGroupName, int additionalCreationFlags = 0) {
		throw new NotImplementedException();
	}

	public bool InitRenderState(IShader shader, IMaterialVar[] shaderParams, ref ShaderRenderState renderState, ReadOnlySpan<char> materialName) {
		Assert(RenderState == null);
		InitRenderStateFlags(ref renderState, shaderParams);
		return true;
	}

	public const int SNAPSHOT_COUNT_NORMAL = 16;
	public const int SNAPSHOT_COUNT_EDITOR = 32;
	public int SnapshotTypeCount() => MaterialSystem.CanUseEditorMaterials() ? SNAPSHOT_COUNT_EDITOR : SNAPSHOT_COUNT_NORMAL;

	private void InitRenderStateFlags(ref ShaderRenderState renderState, IMaterialVar[] shaderParams) {
		renderState.Flags = 0;
		renderState.Flags &= ~ShaderRenderState.SHADER_OPACITY_MASK;
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

	int vertexShaderIndex;
	int pixelShaderIndex;

	internal void SetVertexShader(in VertexShaderHandle vertexShader) {
		if(vertexShader == VertexShaderHandle.INVALID) {
			SetVertexShaderState(0);
			return;
		}

		int vshIndex = vertexShaderIndex;
		Assert(vshIndex >= 0);
		if (vshIndex < 0)
			vshIndex = 0;
	}

	internal void SetPixelShader(in PixelShaderHandle pixelShader) {

	}

	public void Init() {

	}

	Dictionary<ulong, VertexShaderHandle> vshs = [];
	Dictionary<ulong, PixelShaderHandle> pshs = [];


	public unsafe VertexShaderHandle LoadVertexShader(ReadOnlySpan<char> name) {
		ulong symbol = name.Hash();
		if(vshs.TryGetValue(symbol, out VertexShaderHandle value))
			return value;

		using IFileHandle? handle = MaterialSystem.FileSystem.Open($"shaders/{name}", FileOpenOptions.Read, "game");
		if (handle == null)
			return VertexShaderHandle.INVALID;

		Span<byte> source = stackalloc byte[(int)handle.Stream.Length];
		int read = handle.Stream.Read(source);
		uint pShader = 0;
		fixed (byte* pSrc = source)
			pShader = glCreateShaderProgramv(GL_VERTEX_SHADER, 1, &pSrc);

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
		fixed (byte* pSrc = source)
			pShader = glCreateShaderProgramv(GL_VERTEX_SHADER, 1, &pSrc);

		PixelShaderHandle psh = new((nint)pShader);
		pshs[symbol] = psh;
		return psh;
	}
}