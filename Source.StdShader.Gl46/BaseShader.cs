using Source.Common.Engine;
using Source.Common.MaterialSystem;
using Source.Common.ShaderAPI;
using Source.Common.ShaderLib;

using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;

namespace Source.StdShader.Gl46;

public abstract class BaseShader : IShader
{
	[Imported] internal IMaterialSystemHardwareConfig HardwareConfig;
	[Imported] internal IShaderSystem ShaderSystem;

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

	[MemberNotNullWhen(true, nameof(ShaderShadow))]
	internal static bool IsSnapshotting() {
		return ShaderShadow != null;
	}
	public bool TextureIsTranslucent(int textureVar = -1, bool isBaseTexture = true) {
		if (textureVar < 0)
			return false;

		IMaterialVar[] shaderParams = Params!;
		if (shaderParams[textureVar].GetVarType() == MaterialVarType.Texture) {
			if (!isBaseTexture) {
				return shaderParams[textureVar].GetTextureValue()!.IsTranslucent();
			}
			else {
				// Override translucency settings if this flag is set.
				if (IsFlagSet(shaderParams, MaterialVarFlags.OpaqueTexture))
					return false;

				if ((CurrentMaterialVarFlags() & (int)(MaterialVarFlags.SelfIllum | MaterialVarFlags.BaseAlphaEnvMapMask)) == 0) {
					if ((CurrentMaterialVarFlags() & (int)MaterialVarFlags.Translucent) != 0 ||
						(CurrentMaterialVarFlags() & (int)MaterialVarFlags.AlphaTest) != 0) {
						return shaderParams[textureVar].GetTextureValue()!.IsTranslucent();
					}
				}
			}
		}

		return false;
	}
	public void DefaultFog() {
		if ((CurrentMaterialVarFlags() & (int)MaterialVarFlags.Additive) != 0) {
			FogToBlack();
		}
		else {
			FogToFogColor();
		}
	}

	private void DisableFog() {
		Assert(IsSnapshotting());
		ShaderShadow.FogMode(ShaderFogMode.Disabled);
	}
	private void FogToBlack() {
		Assert(IsSnapshotting());
		if ((CurrentMaterialVarFlags() & (int)MaterialVarFlags.NoFog) == 0) {
			ShaderShadow.FogMode(ShaderFogMode.Black);
		}
		else {
			ShaderShadow.FogMode(ShaderFogMode.Disabled);
		}
	}

	private void FogToFogColor() {
		Assert(IsSnapshotting());
		if ((CurrentMaterialVarFlags() & (int)MaterialVarFlags.NoFog) == 0) {
			ShaderShadow.FogMode(ShaderFogMode.FogColor);
		}
		else {
			ShaderShadow.FogMode(ShaderFogMode.Disabled);
		}
	}

	public void SetAdditiveBlendingShadowState(int textureVar = -1, bool isBaseTexture = true) {
		// Either we've got a constant modulation
		bool isTranslucent = IsAlphaModulating();

		// Or we've got a vertex alpha
		isTranslucent = isTranslucent || ((CurrentMaterialVarFlags() & (int)MaterialVarFlags.VertexAlpha) != 0);

		// Or we've got a texture alpha
		isTranslucent = isTranslucent || (TextureIsTranslucent(textureVar, isBaseTexture) &&
										   (CurrentMaterialVarFlags() & (int)MaterialVarFlags.AlphaTest) == 0);

		if (isTranslucent) {
			EnableAlphaBlending(ShaderBlendFactor.SrcAlpha, ShaderBlendFactor.OneMinusSrcAlpha);
		}
		else {
			DisableAlphaBlending();
		}
	}

	private void EnableAlphaBlending(ShaderBlendFactor src, ShaderBlendFactor dst) {
		Assert(IsSnapshotting());
		ShaderShadow.EnableBlending(true);
		ShaderShadow.BlendFunc(src, dst);
		ShaderShadow.EnableDepthWrites(false);
	}

	private void DisableAlphaBlending() {
		Assert(IsSnapshotting());
		ShaderShadow.EnableBlending(false);
	}

	public bool IsAlphaModulating() => (ModulationFlags & (int)ShaderUsing.AlphaModulation) != 0;
	public bool IsColorModulating() => (ModulationFlags & (int)ShaderUsing.ColorModulation) != 0;

	public void SetNormalBlendingShadowState(int textureVar = -1, bool isBaseTexture = true) {
		Assert(IsSnapshotting());

		// Either we've got a constant modulation
		bool isTranslucent = IsAlphaModulating();

		// Or we've got a vertex alpha
		isTranslucent = isTranslucent || ((CurrentMaterialVarFlags() & (int)MaterialVarFlags.VertexAlpha) != 0);

		// Or we've got a texture alpha
		isTranslucent = isTranslucent || (TextureIsTranslucent(textureVar, isBaseTexture) &&
										   (CurrentMaterialVarFlags() & (int)MaterialVarFlags.AlphaTest) == 0);

		if (isTranslucent) {
			EnableAlphaBlending(ShaderBlendFactor.SrcAlpha, ShaderBlendFactor.OneMinusSrcAlpha);
		}
		else {
			DisableAlphaBlending();
		}
	}

	public void SetDefaultBlendingShadowState(int textureVar = -1, bool isBaseTexture = true) {
		if ((CurrentMaterialVarFlags() & (int)MaterialVarFlags.Additive) != 0) {
			SetAdditiveBlendingShadowState(textureVar, isBaseTexture);
		}
		else {
			SetNormalBlendingShadowState(textureVar, isBaseTexture);
		}
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
		if (Params == null || envmapVar == -1)
			return;

		IMaterialVar nameVar = Params[envmapVar];
		if (nameVar != null && nameVar.IsDefined()) {
			// TODO: ShaderInit.LoadCubeMap(Params, nameVar, additionalCreationFlags);
		}
	}

	protected void LoadTexture(int textureVar, int additionalCreationFlags = 0) {
		if (Params == null || textureVar == -1)
			return;

		IMaterialVar nameVar = Params[textureVar];
		if (nameVar != null && nameVar.IsDefined()) {
			ShaderInit!.LoadTexture(nameVar, TextureGroupName, additionalCreationFlags);
		}
	}
}