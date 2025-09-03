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

public enum ShaderParamFlags
{
	NotEditable = 0x1,
	/// <summary>
	/// Marks the standard parameter as non-uploadable - ie, its upload to the GPU is handled by some other component.
	/// </summary>
	DoNotUpload = 0x2
}