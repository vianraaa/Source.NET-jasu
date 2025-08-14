using Source.Common.ShaderLib;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source.StdShader.Gl46;

public struct VertexLitGeneric_Gl46_Vars
{
	public ShaderMaterialVars BaseTexture;
	public ShaderMaterialVars BaseTextureFrame;
	public ShaderMaterialVars BaseTextureTransform;
	public int Albedo;
}
