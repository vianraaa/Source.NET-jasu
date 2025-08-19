using Source.Common.MaterialSystem;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source.StdShader.Gl46;

public static class VertexLitGeneric_Gl46_Helper
{
	public static void InitParams(BaseVSShader shader, Span<IMaterialVar> parms, ReadOnlySpan<char> materialName, bool vertexLitGeneric, in VertexLitGeneric_Gl46_Vars info) {
		
	}

	public static void Init(BaseVSShader shader, Span<IMaterialVar> parms, bool vertexLitGeneric, in VertexLitGeneric_Gl46_Vars info) {

	}

	public static void Draw(BaseVSShader shader, Span<IMaterialVar> parms, bool vertexLitGeneric, in VertexLitGeneric_Gl46_Vars info, VertexCompressionType vertexCompression) {
		bool hasFlashlight = vertexLitGeneric && false; // working on this later
		Draw_Internal(shader, parms, vertexLitGeneric, hasFlashlight, in info, vertexCompression);
	}

	private static void Draw_Internal(BaseVSShader shader, Span<IMaterialVar> parms, bool vertexLitGeneric, bool hasFlashlight, in VertexLitGeneric_Gl46_Vars info, VertexCompressionType vertexCompression) {
		
	}
}
