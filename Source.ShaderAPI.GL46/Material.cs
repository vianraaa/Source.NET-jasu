using Source.Common.Formats.Keyvalues;
using Source.Common.MaterialSystem;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source.MaterialSystem;

public enum MaterialFlags : ushort {
	NeedsWhiteLightmap = 0x1,
	IsPrecached = 0x2,
	VarsIsPrecached = 0x4,
	ValidRenderState = 0x8,
	IsManuallyCreated = 0x10,
	UsesUNCFilename = 0x20,
	IsPReloaded = 0x40,
	ArtificalRefCount = 0x80,
}

public class Material : IMaterialInternal
{
	public Material(ReadOnlySpan<char> materialName, ReadOnlySpan<char> textureGroupName, KeyValues? keyValues) {
		name = new(materialName);
		texGroupName = new(textureGroupName);
		this.keyValues = keyValues;
		if (keyValues != null) {
			flags |= MaterialFlags.IsManuallyCreated;
		}
	}

	public string GetName() {
		return name;
	}

	public bool IsManuallyCreated() {
		return (flags & MaterialFlags.IsManuallyCreated) != 0;
	}

	MaterialFlags flags;
	string name;
	string texGroupName;
	IShader shader;
	KeyValues? keyValues;
}