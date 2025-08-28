using CommunityToolkit.HighPerformance;

using Raylib_cs;

using Source.Common.MaterialSystem;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Source.MaterialSystem;

public struct MaterialLookup(IMaterialInternal? material, ulong symbol, bool manuallyCreated) {
	public readonly ulong Hash() {
		Span<byte> ugh = stackalloc byte[9];
		Span<ulong> sym = [symbol]; MemoryMarshal.Cast<ulong, byte>(sym).CopyTo(ugh);
		ugh[8] = (byte)(manuallyCreated ? 1 : 0);
		return ugh.Hash();
	}

	public readonly IMaterialInternal? Material => material;
	public readonly ulong Symbol => symbol;
	public readonly bool ManuallyCreated => manuallyCreated;
}

public class MaterialDict {
	Dictionary<ulong, MaterialLookup> Dict = [];
	public IMaterialInternal? FindMaterial(ReadOnlySpan<char> name, bool manuallyCreated) {
		MaterialLookup lookup = new(null, name.Hash(), manuallyCreated);
		if (Dict.TryGetValue(lookup.Hash(), out MaterialLookup mat))
			return mat.Material;
		return null;
	}


	public void AddMaterialToMaterialList(IMaterialInternal material) {
		MaterialLookup lookup = new(material, material.GetName().Hash(), material.IsManuallyCreated());
		Dict[lookup.Hash()] = lookup;
	}
}

public interface IMaterialInternal : IMaterial
{
	void DrawMesh(VertexCompressionType vertexCompressionType);
	ReadOnlySpan<char> GetName();
	IMaterialInternal GetRealTimeVersion();
	VertexFormat GetVertexFormat();
	bool IsManuallyCreated();
	bool IsPrecached();
	bool IsRealTimeVersion();
	bool IsUsingVertexID();
	void Precache();
}
