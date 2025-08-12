using Source.Common.MaterialSystem;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source.Common.ShaderLib;

public enum ShaderMaterialVars {
	Flags = 0,
	FlagsDefined,
	FLags2,
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

public enum BlendType {
	None,
	Blend,
	Add,
	BlendAdd
}

public class BaseShader : IShader {
	public static IMaterialVar[] Params;

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
}