using Source.Common.MaterialSystem;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Source;

public static class MaterialMacros
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static VertexFormat VERTEX_BONEWEIGHT(int n) => (VertexFormat)(((ulong)n) << VertexFormatFlags.VertexBoneWeightBit);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static VertexFormat VERTEX_USERDATA_SIZE(int n) => (VertexFormat)(((ulong)n) << VertexFormatFlags.UserDataSizeBit);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static VertexFormat VERTEX_TEXCOORD_MASK(int coord) => (VertexFormat)((0x7UL) << (VertexFormatFlags.TexCoordSizeBit + 3 * coord));
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static VertexFormat VERTEX_TEXCOORD_SIZE(int index, int numCoords) {
		ulong n64 = (ulong)numCoords;
		int nshift = VertexFormatFlags.TexCoordSizeBit + (3 * index);
		return (VertexFormat)(n64 << nshift);
	}
	public static bool IsPlatformOpenGL() => true;
}
