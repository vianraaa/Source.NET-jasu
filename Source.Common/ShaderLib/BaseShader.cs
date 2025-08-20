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

public class BasePerMaterialContextData {
	public uint VarChangeID;
	public bool MaterialVarsChanged;
	public BasePerMaterialContextData() {
		MaterialVarsChanged = true;
		VarChangeID = 0xffffffff;
	}
}