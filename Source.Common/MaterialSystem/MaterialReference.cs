using Source.Common.Formats.Keyvalues;
using Source.Common.Utilities;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Source.Common.MaterialSystem;

public class MaterialReference : Reference<IMaterial>
{
	readonly IMaterialSystem materials = Singleton<IMaterialSystem>();

	public void Init(ReadOnlySpan<char> materialName, ReadOnlySpan<char> textureGroupName, bool complain = true) => throw new NotImplementedException();
	public void Init(ReadOnlySpan<char> materialName, KeyValues keyValues) => throw new NotImplementedException();
	public void Init(MaterialReference reference) => this.reference = reference.reference;
	public void Init(ReadOnlySpan<char> materialName, ReadOnlySpan<char> textureGroupName, KeyValues keyValues) {
		IMaterial? mat = materials.FindProceduralMaterial(materialName, textureGroupName, keyValues);
		Assert(mat != null);
		Init(mat);
	}

	void Init(IMaterial material) {
		if (reference != material) {
			Shutdown();
			reference = material;
		}
	}

	private void Shutdown(bool deleteIfUnreferenced = false) {
		if (reference != null && materials != null) {
			reference = null;
		}
	}
}