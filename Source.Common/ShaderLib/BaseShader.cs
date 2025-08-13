using Source.Common.MaterialSystem;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source.Common.ShaderLib;

public enum ShaderMaterialVars
{
	Flags = 0,
	FlagsDefined,
	Flags2,
	FlagsDefined2,
	Color,
	Alpha,
	BaseTexture,
	Frame,
	BaseTextureTransform,
	FlashLightTexture,
	FlashLightTextureFrame,
	Color2,
	SRGBTint,

	Count,
}

public enum BlendType
{
	None,
	Blend,
	Add,
	BlendAdd
}
public enum ShaderFlags
{
	NotEditable = 0x1
}
public enum ShaderParamFlags
{
	NotEditable = 0x1
}

public abstract class BaseShader : IShader
{
	static IMaterialVar[]? Params;
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

	public int GetNumParams() {
		return (int)ShaderMaterialVars.Count;
	}

	public ReadOnlySpan<char> GetParamName(int paramIndex) {
		Dbg.Assert(paramIndex < (int)ShaderMaterialVars.Count);
		return StandardParams[paramIndex].Name;
	}

	public ReadOnlySpan<char> GetParamHelp(int paramIndex) {
		Dbg.Assert(paramIndex < (int)ShaderMaterialVars.Count);
		return StandardParams[paramIndex].Help;
	}

	public ShaderParamType GetParamType(int paramIndex) {
		Dbg.Assert(paramIndex < (int)ShaderMaterialVars.Count);
		return StandardParams[paramIndex].Type;
	}

	public ReadOnlySpan<char> GetParamDefault(int paramIndex) {
		Dbg.Assert(paramIndex < (int)ShaderMaterialVars.Count);
		return StandardParams[paramIndex].DefaultValue;
	}

	public abstract string? GetFallbackShader(IMaterialVar[] vars);

	public void InitShaderParams(IMaterialVar[] vars, string materialName) {
		Dbg.Assert(Params == null);
		Params = vars;
		OnInitShaderParams(vars, materialName);
		Params = null;
	}

	protected virtual void OnInitShaderParams(IMaterialVar[] vars, string materialName) {

	}
}