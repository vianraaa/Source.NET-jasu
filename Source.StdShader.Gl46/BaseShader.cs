using Source.Common.Engine;
using Source.Common.MaterialSystem;
using Source.Common.ShaderAPI;
using Source.Common.ShaderLib;

namespace Source.StdShader.Gl46;

public abstract class BaseShader : IShader
{
	[Imported] internal IMaterialSystemHardwareConfig HardwareConfig;

	internal static IMaterialVar[]? Params;
	internal static int ModulationFlags;
	internal static IShaderInit? ShaderInit;
	internal static IShaderDynamicAPI? ShaderAPI;
	internal static IShaderShadow? ShaderShadow;
	internal static string? TextureGroupName;

	static ShaderParamInfo[] StandardParams = [
		new(){ Name = "$flags",                    Help = "flags",            Type = ShaderParamType.Integer,DefaultValue =  "0", Flags = ShaderParamFlags.NotEditable },
		new(){ Name = "$flags_defined",            Help = "flags_defined",    Type = ShaderParamType.Integer, DefaultValue = "0", Flags =ShaderParamFlags.NotEditable },
		new(){ Name = "$flags2",                   Help = "flags2",           Type = ShaderParamType.Integer, DefaultValue = "0", Flags =ShaderParamFlags.NotEditable },
		new(){ Name = "$flags_defined2",           Help = "flags2_defined",   Type = ShaderParamType.Integer, DefaultValue = "0", Flags =ShaderParamFlags.NotEditable },
		new(){ Name = "$color",                    Help = "color",            Type = ShaderParamType.Color,   DefaultValue = "[1 1 1]", Flags =0 },
		new(){ Name = "$alpha",                    Help = "alpha",            Type = ShaderParamType.Float,  DefaultValue =  "1.0", Flags =0 },
		new(){ Name = "$basetexture",              Help = "Base Texture with lighting built in", Type = ShaderParamType.Texture,DefaultValue = "shadertest/BaseTexture", Flags =0 },
		new(){ Name = "$frame",                    Help = "Animation Frame", Type =  ShaderParamType.Integer,  DefaultValue ="0", Flags =0 },
		new(){ Name = "$basetexturetransform",     Help = "Base Texture Texcoord Transform",Type = ShaderParamType.Matrix, DefaultValue = "center .5 .5 scale 1 1 rotate 0 translate 0 0", Flags =0 },
		new(){ Name = "$flashlighttexture",        Help = "flashlight spotlight shape texture", Type = ShaderParamType.Texture, DefaultValue ="effects/flashlight001", Flags =ShaderParamFlags.NotEditable },
		new(){ Name = "$flashlighttextureframe",   Help = "Animation Frame for $flashlight",  Type = ShaderParamType.Integer,DefaultValue = "0", Flags =ShaderParamFlags.NotEditable },
		new(){ Name = "$color2",                   Help = "color2",           Type = ShaderParamType.Color,  DefaultValue =  "[1 1 1]",Flags = 0 },
		new(){ Name = "$srgbtint",                 Help = "tint value to be applied when running on new-style srgb parts",            Type = ShaderParamType.Color,   DefaultValue = "[1 1 1]", Flags =0 },
	];

	public string GetName() => GetType().Name;
	public virtual int GetFlags() => 0;

	public int CurrentMaterialVarFlags() {
		return Params[(int)ShaderMaterialVars.Flags].GetIntValue();
	}

	public bool IsWhite(int colorVar) {
		if (colorVar < 0)
			return true;

		if (!Params[colorVar].IsDefined())
			return true;

		Span<float> color = stackalloc float[3];
		Params[colorVar].GetVecValue(color);
		return color[0] >= 1.0f && color[1] >= 1.0f && color[2] >= 1.0f;
	}

	public virtual int GetNumParams() {
		return (int)ShaderMaterialVars.Count;
	}

	public virtual ReadOnlySpan<char> GetParamName(int paramIndex) {
		Dbg.Assert(paramIndex < (int)ShaderMaterialVars.Count);
		return StandardParams[paramIndex].Name;
	}

	public virtual ReadOnlySpan<char> GetParamHelp(int paramIndex) {
		Dbg.Assert(paramIndex < (int)ShaderMaterialVars.Count);
		return StandardParams[paramIndex].Help;
	}

	public virtual ShaderParamType GetParamType(int paramIndex) {
		Dbg.Assert(paramIndex < (int)ShaderMaterialVars.Count);
		return StandardParams[paramIndex].Type;
	}

	public virtual ReadOnlySpan<char> GetParamDefault(int paramIndex) {
		Dbg.Assert(paramIndex < (int)ShaderMaterialVars.Count);
		return StandardParams[paramIndex].DefaultValue;
	}

	public virtual string? GetFallbackShader(IMaterialVar[] vars) => null;

	public void InitShaderParams(IMaterialVar[] vars, ReadOnlySpan<char> materialName) {
		Assert(Params == null);
		Params = vars;
		OnInitShaderParams(vars, materialName);
		Params = null;
	}

	protected virtual void OnInitShaderParams(IMaterialVar[] vars, ReadOnlySpan<char> materialName) {

	}

	protected virtual void OnInitShaderInstance(IMaterialVar[] vars, IShaderInit shaderInit, ReadOnlySpan<char> materialName) {

	}

	protected virtual void OnDrawElements(IMaterialVar[] vars, IShaderShadow shaderShadow, IShaderDynamicAPI shaderAPI, VertexCompressionType vertexCompression, ref BasePerMaterialContextData contextData) {

	}

	public void DrawElements(IMaterialVar[] vars, IShaderShadow shadow, IShaderDynamicAPI shaderAPI, int modulationFlags, VertexCompressionType vertexCompression, ref BasePerMaterialContextData contextData) {
		Assert(Params == null);
		Params = vars;
		ModulationFlags = modulationFlags;
		ShaderAPI = shaderAPI;
		ShaderShadow = shadow;

		if (IsSnapshotting()) {
			SetInitialShadowState();
		}

		OnDrawElements(vars, shadow, shaderAPI, vertexCompression, ref contextData);

		ModulationFlags = 0;
		Params = null;
		ShaderAPI = null;
		ShaderShadow = null;
		// MeshBuilder = null
	}

	private void SetInitialShadowState() {
		ShaderShadow!.SetDefaultState();
		int flags = Params![(int)ShaderMaterialVars.Flags].GetIntValue();
		if ((flags & (int)MaterialVarFlags.IgnoreZ) != 0) {
			ShaderShadow.EnableDepthTest(false);
			ShaderShadow.EnableDepthWrites(false);
		}

		if ((flags & (int)MaterialVarFlags.Decal) != 0) {
			ShaderShadow.EnablePolyOffset(PolygonOffsetMode.Decal);
			ShaderShadow.EnableDepthWrites(false);
		}

		if ((flags & (int)MaterialVarFlags.NoCull) != 0)
			ShaderShadow.EnableCulling(false);

		if ((flags & (int)MaterialVarFlags.ZNearer) != 0)
			ShaderShadow.DepthFunc(ShaderDepthFunc.Nearer);

		if ((flags & (int)MaterialVarFlags.Wireframe) != 0)
			ShaderShadow.PolyMode(ShaderPolyModeFace.FrontAndBack, ShaderPolyMode.Line);

		if ((flags & (int)MaterialVarFlags.AllowAlphaToCoverage) != 0)
			ShaderShadow.EnableAlphaToCoverage(true);
	}

	private bool IsSnapshotting() {
		return ShaderShadow != null;
	}

	public virtual void InitShaderInstance(IMaterialVar[] shaderParams, IShaderInit shaderInit, ReadOnlySpan<char> materialName, ReadOnlySpan<char> textureGroupName) {
		Assert(Params == null);
		Params = shaderParams;
		ShaderInit = shaderInit;
		TextureGroupName = new(textureGroupName);

		OnInitShaderInstance(shaderParams, shaderInit, materialName);

		TextureGroupName = null;
		Params = null;
		ShaderInit = null;
	}

	protected void LoadCubeMap(int envmapVar) {
		throw new NotImplementedException();
	}

	protected void LoadTexture(int textureVar, int additionalCreationFlags = 0) {
		if (Params == null || textureVar == -1)
			return;

		IMaterialVar nameVar = Params[textureVar];
		if(nameVar != null && nameVar.IsDefined()) {
			ShaderInit!.LoadTexture(nameVar, TextureGroupName, additionalCreationFlags);
		}
	}
}