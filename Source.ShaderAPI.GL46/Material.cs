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
	public readonly MaterialSystem materials;
	public Material(MaterialSystem materials, ReadOnlySpan<char> materialName, ReadOnlySpan<char> textureGroupName, KeyValues? keyValues) {
		this.materials = materials;
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

	public bool IsPrecached() {
		return (flags & MaterialFlags.IsPrecached) != 0;
	}
	public bool IsPrecachedVars() {
		return (flags & MaterialFlags.VarsIsPrecached) != 0;
	}

	public void Precache() {
		if (IsPrecached())
			return;

		if (!PrecacheVars())
			return;
	}

	private bool PrecacheVars(KeyValues? inVmtKeyValues = null, KeyValues? inPatchKeyValues = null, int findContext = 0) {
		if (IsPrecachedVars())
			return true;

		// How should we load VMT includes?
		// How should we allow async?

		bool ok = false;
		bool error = false;
		KeyValues? vmtKeyValues = null;
		KeyValues? patchKeyValues = null;

		if(keyValues != null) {
			vmtKeyValues = keyValues;
			patchKeyValues = new("vmt_patches");
		}
		else if(inVmtKeyValues != null) {
			vmtKeyValues = inVmtKeyValues;
			patchKeyValues = inPatchKeyValues;
		}
		else {
			// includes
			error = true;
		}

		if (!error) {
			flags |= MaterialFlags.IsPrecached;
			KeyValues? fallbackKeyValues = InitializeShader(vmtKeyValues!, patchKeyValues, findContext);
			if(fallbackKeyValues != null) {
				InitializeMaterialProxy(fallbackKeyValues);
				ok = true;
			}
		}

		return ok;
	}

	private void InitializeMaterialProxy(KeyValues fallbackKeyValues) {

	}

	private KeyValues? InitializeShader(KeyValues keyValues, KeyValues? patchKeyValues, int findContext) {
		KeyValues currentFallback = keyValues;
		KeyValues? fallbackSection = null;

		string shaderName = currentFallback.Name;
		if(shaderName == null) {
			Dbg.Warning($"Shader not specified in material {GetName()}\nUsing wireframe instead...\n");
			Dbg.Assert(false);
			shaderName = MissingShaderName();
		}

		IShader shader;
		IMaterialVar[] vars = new IMaterialVar[256];
		string fallbackShaderName = "";
		string fallbackMaterialName = "";

		while (true) {
			shader = materials.texture
		}
	}

	private string MissingShaderName() {
		throw new NotImplementedException();
	}

	MaterialFlags flags;
	string name;
	string texGroupName;
	IShader shader;
	KeyValues? keyValues;
}