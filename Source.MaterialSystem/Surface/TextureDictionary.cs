using Source.Common.GUI;
using Source.Common.MaterialSystem;

using System.Diagnostics.CodeAnalysis;

namespace Source.MaterialSystem.Surface;

public class MatSystemTexture(IMaterialSystem materials) {
	public TextureID ID { get; set; }
	public bool Procedural {get; set;}
	public ulong Hash {get; set;}

	public IMaterial? Material;

	public float Wide, Tall, S0, T0, S1, T1;

	public void SetMaterial(ReadOnlySpan<char> filename) {
		IMaterial? material = materials.FindMaterial(filename, TEXTURE_GROUP_VGUI);
		Material = material;
		if (material == null) {
			Wide = Tall = 0;
			S0 = T0 = 0;
			S1 = T1 = 0;
			return;
		}

		Wide = material.GetMappingWidth();
		Tall = material.GetMappingHeight();

		float pixelCenterX = 0.0f;
		float pixelCenterY = 0.0f;

		if (Wide > 0.0f && Tall > 0.0f) {
			pixelCenterX = 0.5f / Wide;
			pixelCenterY = 0.5f / Tall;
		}

		S0 = pixelCenterX;
		T0 = pixelCenterY;
		S1 = 1.0F - pixelCenterX;
		T1 = 1.0F - pixelCenterY;
	}
}

public class TextureDictionary(IMaterialSystem materials, MatSystemSurface surface)
{
	long idx;
	Dictionary<long, MatSystemTexture> Textures = [];

	public bool IsValidId(in TextureID id, [NotNullWhen(true)] out MatSystemTexture? tex) {
		return Textures.TryGetValue(id.ID, out tex);
	}

	internal void BindTextureToFile(in TextureID id, ReadOnlySpan<char> filename) {
		if(!IsValidId(id, out MatSystemTexture? tex)) {
			Msg($"BindTextureToFile: Invalid texture id for file {filename}\n");
			return;
		}

		ulong curhash = filename.Hash();
		if(tex.Material == null || tex.Hash != curhash) {
			tex.Hash = curhash;
			tex.SetMaterial(filename);
		}
	}

	internal TextureID CreateTexture(bool procedural) {
		long idx = this.idx++;
		MatSystemTexture texture = new(materials);
		texture.Procedural = procedural;
		texture.ID = idx;
		Textures[idx] = texture;
		return idx;
	}

	internal void DestroyTexture(in TextureID id) {

	}

	internal IMaterial? GetTextureMaterial(in TextureID id) {
		if (!IsValidId(id, out MatSystemTexture? tex))
			return null;
		return tex.Material;
	}

	internal void GetTextureTexCoords(in TextureID id, out float s0, out float t0, out float s1, out float t1) {
		if (!IsValidId(id, out MatSystemTexture? tex)) {
			s0 = t0 = 0.0f;
			s1 = t1 = 1.0f;
			return;
		}

		s0 = tex.S0;
		t0 = tex.T0;
		s1 = tex.S1;
		t1 = tex.T1;
	}
}
