using Source.Common.Formats.Keyvalues;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Source.Common.MaterialSystem;

public abstract class Reference<T> where T : class
{
	protected T? reference;
	public static implicit operator T?(Reference<T> reference) => reference.reference;

	public bool IsValid() => reference != null;
}

public class MaterialReference : Reference<IMaterial> {
	IMaterialSystem? materials;
	public void Init(IMaterialSystem materials, ReadOnlySpan<char> materialName, ReadOnlySpan<char> textureGroupName, bool complain = true) => throw new NotImplementedException();
	public void Init(IMaterialSystem materials, ReadOnlySpan<char> materialName, KeyValues keyValues) => throw new NotImplementedException();
	public void Init(IMaterialSystem materials, MaterialReference reference) => this.reference = reference.reference;
	public void Init(IMaterialSystem materials, ReadOnlySpan<char> materialName, ReadOnlySpan<char> textureGroupName, KeyValues keyValues) {
		IMaterial? mat = materials.FindProceduralMaterial(materialName, textureGroupName, keyValues);
		Assert(mat != null);
		Init(materials, mat);
	}
	void Init(IMaterialSystem materials, IMaterial material) {
		this.materials = materials;
		if (reference != material) {
			Shutdown();
			reference = material;
		}
	}

	private void Shutdown() {
		if(reference != null && materials != null) {
			reference = null;
		}
	}
}